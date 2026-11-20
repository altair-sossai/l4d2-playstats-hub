using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class SkeetHurtEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.SkeetHurt;
    public LivePlayer? Hunter { get; set; }
    public SkeetType SkeetType { get; set; }
    public int Damage { get; set; }
    public bool IsOverkill { get; set; }
}
