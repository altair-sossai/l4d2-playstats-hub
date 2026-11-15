using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class HunterHighPounceEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.HunterHighPounce;
    public LivePlayer? Victim { get; set; }
    public double CalculatedDamage { get; set; }
    public double Height { get; set; }
}
