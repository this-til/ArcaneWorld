using System;

namespace EventBus;

public interface IEventExceptionHandle {

    /// <summary>
    /// 进行抛出异常的处理
    /// 返回true表示类型已经被处理
    /// </summary>
    bool doCatch(IEventBus iEventBus, IEventInvoke eventInvoke, Event @event, Exception exception);



}

public class LogExceptionHandle : IEventExceptionHandle {

    public static LogExceptionHandle instance { get; } = new LogExceptionHandle();

    // ReSharper disable Unity.PerformanceAnalysis
    public bool doCatch(IEventBus iEventBus, IEventInvoke eventInvoke, Event @event, Exception exception) {
        iEventBus.log?.error($"事件处理异常 - 处理器: {eventInvoke}, 事件: {@event.GetType().Name}, 异常: {exception.Message}", exception);
        return true;
    }

}