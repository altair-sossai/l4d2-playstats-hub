using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class SkeetEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.Skeet;
    public override int Stars => IsTeamSkeet || Hunter?.IsBot == true ? 1 : base.Stars;
    public LivePlayer? Hunter { get; set; }
    public SkeetType SkeetType { get; set; }
    public bool IsTeamSkeet { get; set; }
}