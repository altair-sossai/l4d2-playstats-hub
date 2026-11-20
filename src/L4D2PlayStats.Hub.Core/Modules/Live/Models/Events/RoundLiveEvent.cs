using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class RoundLiveEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.RoundLive;
    public bool SecondHalf { get; set; }
}
