using System.Reflection;
using CakeToolset.Attribute;
using CakeToolset.Global;
using CakeToolset.Log;
using CommonUtil.Extensions;
using Godot;

namespace CakeToolset.Global.Component;

[Tool]
public abstract partial class EventBusHold : Node, IGlobalComponent {

    public static EventBus.EventBus eventBus { get; private set; } = null!;

    public void initialize() {
        eventBus = new EventBus.EventBus(
            new EventBus.EventBus.EventBusBuilder() {
                log = LogManager.GetLogger("EventBus")
            }
        );

        GlobalComponentLoader.instance.eventBusSubscriberAttributeTypes
            .Peek(t => eventBus.put(t.type))
            .End();

        RegisterSystemHold.registerSystem.manageList
            .Where(r => r.GetType().GetCustomAttribute<EventBusSubscriberAttribute>() != null)
            .Peek(eventBus.put)
            .End();

        RegisterSystemHold.registerSystem.registerBasicsSortedSet
            .Where(r => r.GetType().GetCustomAttribute<EventBusSubscriberAttribute>() != null)
            .Peek(eventBus.put)
            .End();

    }

    public void terminate() {
        eventBus?.Dispose();
        eventBus = null!;
    }
    
    public int priority => 1 << 23;

}
