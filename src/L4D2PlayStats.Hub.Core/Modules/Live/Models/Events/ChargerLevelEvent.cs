using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class ChargerLevelEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.ChargerLevel;
    public LivePlayer? Charger { get; set; }
}