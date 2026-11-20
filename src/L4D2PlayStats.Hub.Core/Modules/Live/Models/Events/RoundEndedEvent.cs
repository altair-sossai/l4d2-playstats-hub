using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

public class RoundEndedEvent : GameEvent
{
    public override GameEventType EventType => GameEventType.RoundEnded;
    public int SurvivorScore { get; set; }
    public int InfectedScore { get; set; }
    public int Difference => Math.Abs(SurvivorScore - InfectedScore);
}