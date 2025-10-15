using System.Reflection;
using ArcaneWorld.Attribute;
using ArcaneWorld.Util;
using CommonUtil.Extensions;
using Godot;

namespace ArcaneWorld.Global;

public partial class EventBusHold : Node {

    public static EventBus.EventBus eventBus { get; private set; } = null!;

    public override void _Ready() {
        base._Ready();

        eventBus = new EventBus.EventBus(
            new EventBus.EventBus.EventBusBuilder() {
                log = log4net.LogManager.GetLogger("EventBus")
            }
        );

        AssemblyLoadManage.instance.eventBusSubscriberAttributeTypes
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

}
