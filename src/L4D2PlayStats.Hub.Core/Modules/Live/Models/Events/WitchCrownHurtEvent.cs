using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class WitchCrownHurtEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.WitchCrownHurt;
    public int Damage { get; set; }
    public int ChipDamage { get; set; }
}