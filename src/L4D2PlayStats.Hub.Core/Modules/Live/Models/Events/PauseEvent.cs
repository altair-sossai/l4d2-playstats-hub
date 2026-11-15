using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class PauseEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.Pause;
    public bool Paused { get; set; }
}
