using System;

namespace EventBus;

[AttributeUsage(AttributeTargets.Method)]
public class EventAttribute : Attribute {

    /// <summary>
    /// 优先级
    /// </summary>
    public int priority;

    /// <summary>
    /// 指定并发组
    /// 该事件仅对 IAsyncEvent 有效
    /// </summary>
    public string concurrencyGroup = String.Empty;

}

/// <summary>
/// 排除供应商
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EventSupplierExcludeAttribute : Attribute {

    public bool excludeStatic = true;

    public bool excludeInstance = true;

}