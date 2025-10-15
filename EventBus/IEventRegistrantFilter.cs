using System;
using System.Reflection;

namespace EventBus;

/// <summary>
/// 一个注册者过滤器
/// </summary>
public interface IEventRegistrantFilter {

    /// <summary>
    /// 如果返回true代表它被过滤掉了，它无法被注册进系统
    /// </summary>
    bool isFilter(IEventBus eventBus, object registrant);

}

public class EventRegistrantExcludeAttributeFilter : IEventRegistrantFilter {

    public static EventRegistrantExcludeAttributeFilter instance { get; } = new EventRegistrantExcludeAttributeFilter();

    public bool isFilter(IEventBus eventBus, object registrant) {
        EventSupplierExcludeAttribute? eventSupplierExcludeAttribute = (registrant as Type ?? registrant.GetType()).GetCustomAttribute<EventSupplierExcludeAttribute>();
        if (eventSupplierExcludeAttribute is null) {
            return false;
        }
        switch (registrant) {
            case Type:
                return eventSupplierExcludeAttribute.excludeStatic;
            default:
                return eventSupplierExcludeAttribute.excludeInstance;
        }
    }

}

public class EventRegistrantTypeFilter : IEventRegistrantFilter {

    public static EventRegistrantTypeFilter instance { get; } = new EventRegistrantTypeFilter();

    public bool isFilter(IEventBus eventBus, object registrant) {
        if (registrant.GetType().IsPrimitive) {
            if (eventBus.log?.IsWarnEnabled ?? false) {
                eventBus.log?.Warn($"注册项 {registrant.GetType()} 是基础数据类型，他不能被注册");
            }
            return true;
        }

        if (registrant.GetType().IsValueType) {
            if (eventBus.log?.IsWarnEnabled ?? false) {
                eventBus.log?.Warn($"注册项 {registrant.GetType()} 是结构体，他不能被注册");
            }
            return true;
        }

        if (registrant.GetType().IsEnum) {
            if (eventBus.log?.IsWarnEnabled ?? false) {
                eventBus.log?.Warn($"注册项 {registrant.GetType()} 是枚举，他不能被注册");
            }
            return true;
        }

        if (registrant is IEventBus) {
            if (eventBus.log?.IsWarnEnabled ?? false) {
                eventBus.log?.Warn($"注册项 {registrant.GetType()} 是{typeof(IEventBus)}，他不能被注册");
            }
            return true;
        }

        return false;
    }

}
