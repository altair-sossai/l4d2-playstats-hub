using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class CarAlarmTriggeredEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.CarAlarmTriggered;
    public LivePlayer? Infected { get; set; }
    public CarAlarmReason Reason { get; set; }
}
