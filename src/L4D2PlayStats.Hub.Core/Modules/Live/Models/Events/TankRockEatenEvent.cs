using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class TankRockEatenEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.TankRockEaten;
    public LivePlayer? Victim { get; set; }
}