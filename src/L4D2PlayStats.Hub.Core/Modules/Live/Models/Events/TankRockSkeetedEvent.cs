using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class TankRockSkeetedEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.TankRockSkeeted;
    public LivePlayer? Tank { get; set; }
}
