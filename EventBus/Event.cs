using System.Threading;

namespace EventBus;

/// <summary>
/// 事件基类
/// </summary>
public class Event {
        

}

public interface ICancellations {

    /// <summary>
    /// 表明事件是可取消的，是一个基于内部事件状态的
    /// IAsyncEvent 调用时 token 仅表示外部需要取消
    /// 调用该方法时不会锁对象，需要事件内部实现原子化操作
    /// </summary>
    /// <returns></returns>
    bool isCancellations();

}

public interface IAsyncEvent {

    CancellationToken token { get; }

    // ReSharper disable once SuspiciousTypeConversion.Global
    Event toEvent => (Event)this;

    bool isEmptyGroupSynchronization => false;

}

public interface IYieldEvent {

    // ReSharper disable once SuspiciousTypeConversion.Global
    Event toEvent => (Event)this;

}