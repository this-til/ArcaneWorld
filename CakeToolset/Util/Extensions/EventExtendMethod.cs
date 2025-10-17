using CakeToolset.Global.Component;
using EventBus;

namespace CakeToolset.Util.Extensions;

public static class EventExtendMethod {

    public static E onEvent<E>(this E @event) where E : Event {
        return (E)EventBusHold.eventBus.onEvent(@event);
    }

    public static Task onEventAsync(this IAsyncEvent @event) => EventBusHold.eventBus.onEventAsync(@event);

}
