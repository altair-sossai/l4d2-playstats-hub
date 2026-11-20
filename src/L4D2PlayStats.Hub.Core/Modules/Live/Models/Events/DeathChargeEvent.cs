using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class DeathChargeEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.DeathCharge;
    public LivePlayer? Victim { get; set; }
    public double Height { get; set; }
    public double Distance { get; set; }
    public bool WasCarried { get; set; }
}