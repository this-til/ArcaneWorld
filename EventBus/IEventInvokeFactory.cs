using System;
using System.Reflection;

namespace EventBus;

public interface IEventInvokeFactory {

    IEventInvoke? create
    (
        IEventBus eventBus,
        object obj,
        Type eventType,
        Type methodInfoReturnType,
        MethodInfo methodInfo,
        EventAttribute eventAttribute
    );

}

public class DefaultEventInvokeFactory :  IEventInvokeFactory {

    public static DefaultEventInvokeFactory instance { get; } = new DefaultEventInvokeFactory();
        
    public IEventInvoke? create
    (
        IEventBus eventBus,
        object registrant,
        Type eventType,
        Type methodInfoReturnType,
        MethodInfo methodInfo,
        EventAttribute eventAttribute
    ) {
        if (methodInfoReturnType == typeof(void)) {
            return Activator.CreateInstance(typeof(EventInvoke<>).MakeGenericType(eventType), registrant, methodInfo, eventAttribute) as IEventInvoke;
        }
        return Activator.CreateInstance(typeof(EventInvoke<,>).MakeGenericType(eventType, methodInfoReturnType), registrant, methodInfo, eventAttribute) as IEventInvoke;
    }

}