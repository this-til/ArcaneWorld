using System;
using System.Reflection;

namespace EventBus;

public interface IEventInvoke : IPriority {

    /// <summary>
    /// 事件被调用
    /// </summary>
    object? invoke(Event @event);

    /// <summary>
    /// 获取事件的注册者
    /// </summary>
    object registrant { get; }

    /// <summary>
    /// 获取监听的方法
    /// </summary>
    MethodInfo methodInfo { get; }

    /// <summary>
    /// 方法上的注解
    /// </summary>
    EventAttribute eventAttribute { get; }

    /// <summary>
    /// 获取事件类型
    /// </summary>
    Type eventType { get; }

}

public abstract class EventInvoke : IEventInvoke {

    public int priority => eventAttribute.priority;

    public object registrant { get; }

    public MethodInfo methodInfo { get; }

    public EventAttribute eventAttribute { get; }

    public Type eventType { get; }

    protected EventInvoke(object registrant, MethodInfo methodInfo, EventAttribute eventAttribute, Type eventType) {
        this.methodInfo = methodInfo;
        this.registrant = registrant;
        this.eventAttribute = eventAttribute;
        this.eventType = eventType;
    }

    public abstract object? invoke(Event @event);

    public override string ToString() {
        return $"{eventType} -> {registrant} => {methodInfo}";
    }

}

public class EventInvoke<T> : EventInvoke where T : Event {

    public delegate void InvokeProxy(T @event);

    protected readonly InvokeProxy proxy;

    public EventInvoke(object registrant, MethodInfo methodInfo, EventAttribute eventAttribute) : base(registrant, methodInfo, eventAttribute, typeof(T)) {
        this.proxy = (InvokeProxy)methodInfo.CreateDelegate(
            typeof(InvokeProxy),
            methodInfo.IsStatic
                ? null
                : registrant
        );
    }

    public override object? invoke(Event @event) {
        proxy((T)@event);
        return null;
    }

}

public class EventInvoke<T, R> : EventInvoke where T : Event {

    public delegate R InvokeProxy(T @event);

    protected readonly InvokeProxy proxy;

    public EventInvoke(object registrant, MethodInfo methodInfo, EventAttribute eventAttribute) : base(registrant, methodInfo, eventAttribute, typeof(T)) {
        this.proxy = (InvokeProxy)methodInfo.CreateDelegate(
            typeof(InvokeProxy),
            methodInfo.IsStatic
                ? null
                : registrant
        );
    }

    public override object? invoke(Event @event) => proxy((T)@event);

}