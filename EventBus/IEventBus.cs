using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommonUtil;
using CommonUtil.Log;

namespace EventBus;

public interface IEventBus {

    ILog? log { get; }

    /// <summary>
    /// 注册一个事件监听者
    /// </summary>
    void put(object registered);

    /// <summary>
    /// 删除一个事件监听者
    /// </summary>
    void remove(object registered);

    /// <summary>
    /// 发布一个事件
    /// </summary>
    Event onEvent(Event @event);

    public Task onEventAsync(IAsyncEvent asyncEvent);

    public IEnumerable onEventYield(IYieldEvent yieldEvent);

}

public class EventBus : IEventBus, IDisposable {

    protected readonly IEnumerable<IEventRegistrantFilter> eventRegistrantFilterList;

    protected readonly IEnumerable<IEventInvokeFilter> eventTriggerFilterList;

    protected readonly IEnumerable<IEventInvokeFactory> eventTriggerFactoryList;

    protected readonly IEnumerable<IEventExceptionHandle> eventExceptionHandleList;

    protected readonly IEnumerable<IConvertAwait> convertAwaitList;

    public ILog? log { get; }

    protected readonly ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    private bool disposed = false;

    /// <summary>
    /// 所有的注册者
    /// </summary>
    protected readonly Dictionary<object, IReadOnlyList<IEventInvoke>> registrantMap = new Dictionary<object, IReadOnlyList<IEventInvoke>>();

    /// <summary>
    /// 事件触发器，调用节点
    /// </summary>
    protected readonly Dictionary<Type, EventTrigger> eventTriggerMap = new Dictionary<Type, EventTrigger>();

    /// <summary>
    /// 值是键的直接派生类
    /// </summary>
    protected readonly Dictionary<Type, List<Type>> sonTypeMap = new Dictionary<Type, List<Type>>();

    /// <summary>
    /// 
    /// </summary>
    protected readonly ConcurrentDictionary<Type, IConvertAwait?> convertAwaitMap = new ConcurrentDictionary<Type, IConvertAwait?>();

    public EventBus(EventBusBuilder eventBusBuilder) {
        eventRegistrantFilterList = new ReadOnlyCollection<IEventRegistrantFilter>(eventBusBuilder.eventRegistrantFilterList);
        eventTriggerFilterList = new ReadOnlyCollection<IEventInvokeFilter>(eventBusBuilder.eventTriggerFilterList);
        eventTriggerFactoryList = new ReadOnlyCollection<IEventInvokeFactory>(eventBusBuilder.eventTriggerFactoryList);
        eventExceptionHandleList = new ReadOnlyCollection<IEventExceptionHandle>(eventBusBuilder.eventExceptionHandleList);
        convertAwaitList = new ReadOnlyCollection<IConvertAwait>(eventBusBuilder.convertAwaitList);

        log = eventBusBuilder.log;

        eventTriggerMap.Add(typeof(Event), new EventTrigger(this, typeof(Event)));

    }

    public void put(object registered) {

        IEventRegistrantFilter? eventRegistrantFilter = eventRegistrantFilterList.FirstOrDefault(f => f.isFilter(this, registered));

        if (eventRegistrantFilter != null) {

            if (log?.IsInfoEnabled ?? false) {
                log?.Info($"{registered} 被 {eventRegistrantFilter} 过滤掉了 ");
            }

            return;
        }

        readerWriterLockSlim.EnterWriteLock();

        try {

            if (registrantMap.ContainsKey(registered)) {
                return;
            }

            IReadOnlyList<IEventInvoke> triggers = generateTrigger(registered);
            registrantMap.Add(registered, triggers);

            foreach (IEventInvoke eventInvoke in triggers) {

                putTrigger(eventInvoke, eventInvoke.eventType);

            }

        }
        finally {
            readerWriterLockSlim.ExitWriteLock();
        }

    }

    public void remove(object registered) {

        readerWriterLockSlim.EnterWriteLock();

        try {

            if (!registrantMap.Remove(registered, out IReadOnlyList<IEventInvoke> list)) {
                return;
            }

            foreach (IEventInvoke eventInvoke in list) {

                removeTrigger(eventInvoke, eventInvoke.eventType);

            }

        }
        finally {
            readerWriterLockSlim.ExitWriteLock();
        }

    }

    protected void putTrigger(IEventInvoke eventInvoke, Type evenType) {
        readerWriterLockSlim.EnterWriteLock();

        try {

            if (!eventTriggerMap.TryGetValue(evenType, out EventTrigger? eventTrigger)) {

                eventTrigger = getOrDerivationTrigger(evenType);

            }

            if (eventTrigger.isExecution()) {
                eventTrigger = new EventTrigger(this, evenType);
                eventTriggerMap[evenType] = eventTrigger;
            }

            eventTrigger.add(eventInvoke);

            if (!sonTypeMap.TryGetValue(evenType, out List<Type> sonTypeList)) {
                return;
            }

            foreach (Type type in sonTypeList) {
                putTrigger(eventInvoke, type);
            }

        }
        finally {
            readerWriterLockSlim.ExitWriteLock();
        }
    }

    protected void removeTrigger(IEventInvoke eventInvoke, Type evenType) {
        readerWriterLockSlim.EnterWriteLock();

        try {

            if (!eventTriggerMap.TryGetValue(evenType, out EventTrigger? eventTrigger)) {
                return;
            }

            if (eventTrigger.isExecution()) {
                eventTrigger = new EventTrigger(this, evenType);
                eventTriggerMap[evenType] = eventTrigger;
            }

            eventTrigger.remove(eventInvoke);

            if (!sonTypeMap.TryGetValue(evenType, out List<Type> sonTypeList)) {
                return;
            }

            foreach (Type type in sonTypeList) {
                removeTrigger(eventInvoke, type);
            }

        }
        finally {
            readerWriterLockSlim.ExitWriteLock();
        }

    }

    protected IReadOnlyList<IEventInvoke> generateTrigger(object registered) {
        List<IEventInvoke> list = new List<IEventInvoke>();

        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
        bindingFlags |= registered is Type
            ? BindingFlags.Static
            : BindingFlags.Instance;

        Type registeredType = registered as Type ?? registered.GetType();

        foreach (MethodInfo methodInfo in registeredType.GetMethods(bindingFlags)) {

            EventAttribute? eventAttribute = methodInfo.GetCustomAttribute<EventAttribute>();
            if (eventAttribute is null) {
                continue;
            }

            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            if (parameterInfos.Length != 1) {
                continue;
            }

            if (!typeof(Event).IsAssignableFrom(parameterInfos[0].ParameterType)) {
                continue;
            }

            Type eventType = parameterInfos[0].ParameterType;

            IEventInvokeFilter? eventTriggerFilter = eventTriggerFilterList.FirstOrDefault
            (
                f => f.isFilter(
                    this,
                    registered,
                    registeredType,
                    eventType,
                    methodInfo,
                    eventAttribute
                )
            );

            if (eventTriggerFilter != null) {

                if (log?.IsInfoEnabled ?? false) {
                    log?.Info($"{registeredType}.{methodInfo} was filtered out by {eventTriggerFilter}");
                }

                continue;
            }

            Type methodInfoReturnType = methodInfo.ReturnType;

            IEventInvoke? eventInvoke = eventTriggerFactoryList
                .Select
                (
                    f => f.create(
                        this,
                        registered,
                        eventType,
                        methodInfoReturnType,
                        methodInfo,
                        eventAttribute
                    )
                )
                .FirstOrDefault();

            if (eventInvoke == null) {

                if (log?.IsInfoEnabled ?? false) {
                    log?.Info($"{registeredType}.{methodInfo} was not created {nameof(IEventInvoke)}");
                }

                continue;
            }

            list.Add(eventInvoke);

        }

        return list;
    }

    protected EventTrigger getOrDerivationTrigger(Type eventType) {

        if (eventTriggerMap.TryGetValue(eventType, out EventTrigger? existing)) {
            return existing;
        }

        readerWriterLockSlim.EnterWriteLock();

        try {

            if (eventTriggerMap.TryGetValue(eventType, out existing)) {
                return existing;
            }

            Type baseType = eventType.BaseType!;

            EventTrigger baseTrigger = getOrDerivationTrigger(baseType);
            EventTrigger newTrigger = baseTrigger.derivation(eventType);
            eventTriggerMap.TryAdd(eventType, newTrigger);

            if (!sonTypeMap.TryGetValue(baseType, out List<Type>? sonTypeList)) {
                sonTypeList = new List<Type>();
                sonTypeMap.Add(baseType, sonTypeList);
            }

            sonTypeList.Add(eventType);

            return newTrigger;

        }
        finally {
            readerWriterLockSlim.ExitWriteLock();
        }

    }

    public Event onEvent(Event @event) {
        if (@event is IAsyncEvent) {
            throw new NotSupportedException($"事件 {@event.GetType()} 是 {nameof(IAsyncEvent)}, 请使用 {nameof(onEventAsync)}()");
        }
        if (@event is IYieldEvent) {
            throw new NotSupportedException($"事件 {@event.GetType()} 是 {nameof(IYieldEvent)}, 请使用 {nameof(onEventYield)}()");
        }
        getOrDerivationTrigger(@event.GetType()).onEvent(@event);
        return @event;
    }

    public Task onEventAsync(IAsyncEvent asyncEvent) {
        return getOrDerivationTrigger(asyncEvent.GetType()).onEventAsync(asyncEvent);
    }

    public IEnumerable onEventYield(IYieldEvent yieldEvent) {
        return getOrDerivationTrigger(yieldEvent.GetType()).onEventYield(yieldEvent);
    }

    protected void handleExceptions(Event @event, IEventInvoke eventInvoke, Exception e) {

        _ = this.eventExceptionHandleList
            .FirstOrDefault
            (
                h => h.doCatch(
                    this,
                    eventInvoke,
                    @event,
                    e
                )
            );

    }

    protected IGetAwaiter? getAwaiter(object? obj) {
        if (obj == null) {
            return null;
        }

        Type key = obj.GetType();

        IConvertAwait? convertAwait = convertAwaitMap.GetOrAdd(
            key,
            type => convertAwaitList.FirstOrDefault
            (
                c => c.canConvert(key)
            )
        );

        return convertAwait?.convert(obj);
    }

    public class EventTrigger {

        protected readonly EventBus eventBus;

        protected readonly Type eventType;

        protected List<IEventInvoke> synchronousInvocation = new List<IEventInvoke>();

        protected Dictionary<string, List<IEventInvoke>> parallelCallMap = new Dictionary<string, List<IEventInvoke>>();

        protected volatile int callCounter;

        public EventTrigger(EventBus eventBus, Type eventType) {
            this.eventType = eventType;
            this.eventBus = eventBus;
        }

        public void add(IEventInvoke eventInvoke) {
            synchronousInvocation.addByPriority(eventInvoke);

            if (!parallelCallMap.TryGetValue(eventInvoke.eventAttribute.concurrencyGroup, out List<IEventInvoke> parallelCall)) {
                parallelCall = new List<IEventInvoke>();
                parallelCallMap.Add(eventInvoke.eventAttribute.concurrencyGroup, parallelCall);
            }

            parallelCall.addByPriority(eventInvoke);
        }

        public bool isExecution() {
            return callCounter > 0;
        }

        public void remove(IEventInvoke eventInvoke) {
            synchronousInvocation.removeByPriority(eventInvoke);

            if (parallelCallMap.TryGetValue(eventInvoke.eventAttribute.concurrencyGroup, out List<IEventInvoke> parallelCall)) {
                parallelCall.removeByPriority(eventInvoke);
            }
        }

        public EventTrigger copy() {
            EventTrigger eventTrigger = new EventTrigger(eventBus, eventType);
            eventTrigger.synchronousInvocation.AddRange(synchronousInvocation);
            foreach (KeyValuePair<string, List<IEventInvoke>> keyValuePair in parallelCallMap) {
                eventTrigger.parallelCallMap.Add(keyValuePair.Key, new List<IEventInvoke>(keyValuePair.Value));
            }
            return eventTrigger;
        }

        public EventTrigger derivation(Type eventType) {
            EventTrigger eventTrigger = new EventTrigger(eventBus, eventType);
            eventTrigger.synchronousInvocation.AddRange(synchronousInvocation);
            foreach (KeyValuePair<string, List<IEventInvoke>> keyValuePair in parallelCallMap) {
                eventTrigger.parallelCallMap.Add(keyValuePair.Key, new List<IEventInvoke>(keyValuePair.Value));
            }
            return eventTrigger;
        }

        public void onEvent(Event @event) {

            ICancellations? iCancellations = @event as ICancellations;

            try {
                Interlocked.Increment(ref callCounter);

                foreach (IEventInvoke eventInvoke in synchronousInvocation) {
                    try {
                        if (iCancellations?.isCancellations() ?? false) {
                            break;
                        }
                        eventInvoke.invoke(@event);
                    }
                    catch (Exception e) {
                        eventBus.handleExceptions(@event, eventInvoke, e);
                    }
                }
            }
            finally {
                Interlocked.Decrement(ref callCounter);
            }
        }

        public async Task asyncInvoke(Event @event, IAsyncEvent asyncEvent, IEventInvoke eventInvoke) {

            try {

                if (asyncEvent.token.IsCancellationRequested) {
                    return;
                }

                object? invoke = eventInvoke.invoke(@event);
                IGetAwaiter? getAwaiter = eventBus.getAwaiter(invoke);
                if (getAwaiter != null) {
                    await getAwaiter;
                }

            }
            catch (Exception e) {
                eventBus.handleExceptions(@event, eventInvoke, e);
            }
        }

        public async Task onEventAsync(IAsyncEvent asyncEvent) {

            Event @event = asyncEvent.toEvent;
            ICancellations? iCancellations = asyncEvent as ICancellations;
            CancellationToken asyncEventToken = asyncEvent.token;

            if (asyncEventToken.IsCancellationRequested) {
                return;
            }

            try {
                Interlocked.Increment(ref callCounter);

                await Task.WhenAll
                (
                    parallelCallMap
                        .Select
                        (
                            p => {

                                if (!asyncEvent.isEmptyGroupSynchronization && p.Key.Equals(String.Empty)) {

                                    return Task.WhenAll
                                    (
                                        p.Value.Select
                                        (
                                            eventInvoke => asyncInvoke(@event, asyncEvent, eventInvoke)
                                        )
                                    );

                                }

                                return Task.Run(
                                    async () => {
                                        foreach (IEventInvoke eventInvoke in p.Value) {
                                            if (asyncEventToken.IsCancellationRequested) {
                                                break;
                                            }
                                            if (iCancellations?.isCancellations() ?? false) {
                                                break;
                                            }
                                            await asyncInvoke(@event, asyncEvent, eventInvoke);
                                        }
                                    },
                                    asyncEventToken
                                );

                            }
                        )
                );

            }
            finally {
                Interlocked.Decrement(ref callCounter);
            }

        }

        public IEnumerable onEventYield(IYieldEvent yieldEvent) {

            Event @event = yieldEvent.toEvent;
            ICancellations? iCancellations = yieldEvent as ICancellations;

            try {

                Interlocked.Increment(ref callCounter);

                foreach (IEventInvoke eventInvoke in synchronousInvocation) {

                    object? invoke;

                    try {
                        if (iCancellations?.isCancellations() ?? false) {
                            yield break;
                        }
                        invoke = eventInvoke.invoke(@event);
                    }
                    catch (Exception e) {
                        eventBus.handleExceptions(@event, eventInvoke, e);
                        continue;
                    }

                    IEnumerator? enumerator = null;
                    switch (invoke) {
                        case IEnumerable enumerable:
                            enumerator = enumerable.GetEnumerator();
                            break;
                        case IEnumerator _enumerator:
                            enumerator = _enumerator;
                            break;
                    }

                    if (enumerator == null) {
                        continue;
                    }

                    Exception? throwException = null;

                    while (true) {

                        try {
                            if (!enumerator.MoveNext()) {
                                break;
                            }
                            if (iCancellations?.isCancellations() ?? false) {
                                break;
                            }
                        }
                        catch (Exception e) {
                            throwException = e;
                            break;
                        }

                        yield return enumerator.Current;

                    }

                    (enumerator as IDisposable)?.Dispose();

                    if (throwException != null) {
                        eventBus.handleExceptions(@event, eventInvoke, throwException);
                    }

                }

            }
            finally {
                Interlocked.Decrement(ref callCounter);
            }

        }

    }

    public class EventBusBuilder {

        public List<IEventRegistrantFilter> eventRegistrantFilterList = new List<IEventRegistrantFilter> {
            EventRegistrantExcludeAttributeFilter.instance,
            EventRegistrantTypeFilter.instance
        };

        public List<IEventInvokeFilter> eventTriggerFilterList = new List<IEventInvokeFilter> {
        };

        public List<IEventInvokeFactory> eventTriggerFactoryList = new List<IEventInvokeFactory> {
            DefaultEventInvokeFactory.instance
        };

        public List<IEventExceptionHandle> eventExceptionHandleList = new List<IEventExceptionHandle> {
            LogExceptionHandle.instance
        };

        public List<IConvertAwait> convertAwaitList = new List<IConvertAwait> {
            TaskConvertAwaiter.instance,
            ValueTaskConvertAwaiter.instance,
        };

        public EventBusBuilder addEventRegistrantFilter(IEventRegistrantFilter eventRegistrantFilter) {
            eventRegistrantFilterList.Insert(0, eventRegistrantFilter);
            return this;
        }

        public EventBusBuilder addEventTriggerFilter(IEventInvokeFilter eventInvokeFilter) {
            eventTriggerFilterList.Insert(0, eventInvokeFilter);
            return this;
        }

        public EventBusBuilder addEventTriggerFactory(IEventInvokeFactory eventInvokeFactory) {
            eventTriggerFactoryList.Insert(0, eventInvokeFactory);
            return this;
        }

        public EventBusBuilder addEventExceptionHandle(IEventExceptionHandle eventExceptionHandle) {
            eventExceptionHandleList.Insert(0, eventExceptionHandle);
            return this;
        }

        public ILog? log;

    }

    /// <summary>
    /// 清理 EventBus 资源
    /// </summary>
    public void Dispose() {
        if (disposed) {
            return;
        }

        // 清理托管资源
        readerWriterLockSlim.EnterWriteLock();
        try {
            // 清空所有注册者
            registrantMap.Clear();

            // 清空所有事件触发器
            eventTriggerMap.Clear();

            // 清空类型映射
            sonTypeMap.Clear();

            // 清空转换映射
            convertAwaitMap.Clear();
        }
        finally {
            readerWriterLockSlim.ExitWriteLock();
        }

        // 释放锁
        readerWriterLockSlim.Dispose();

        disposed = true;
        
        GC.SuppressFinalize(this);
    }

}
