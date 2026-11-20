using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class ChargerLevelHurtEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.ChargerLevelHurt;
    public LivePlayer? Charger { get; set; }
    public int Damage { get; set; }
}