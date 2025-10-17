using System.ComponentModel.DataAnnotations;
using CakeToolset.Register;

namespace CakeToolset;

public class Event : EventBus.Event {

    public class ConfigEvent : Event {

        public class ConfigChangeEvent : ConfigEvent {

            [Required]
            public Config overallConfig { get; init; } = null !;

        }

    }

}
