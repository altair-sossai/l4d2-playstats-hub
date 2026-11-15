using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class TongueCutEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.TongueCut;
    public LivePlayer? Smoker { get; set; }
}
