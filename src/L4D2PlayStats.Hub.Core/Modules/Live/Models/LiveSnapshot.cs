using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LiveSnapshot
{
    public LiveConfiguration? Configuration { get; init; }
    public LiveRound? Round { get; init; }
    public LiveScoreboard? Scoreboard { get; init; }
    public IReadOnlyList<LiveSurvivor> Survivors { get; init; } = [];
    public IReadOnlyList<LiveInfected> Infecteds { get; init; } = [];
    public IReadOnlyList<LivePlayer> Spectators { get; init; } = [];
    public bool AnyPlayerConnected => PlayerCount > 0;
    public int PlayerCount => Survivors.Count + Infecteds.Count + Spectators.Count;

    [JsonIgnore]
    public IReadOnlyList<FeedItem> Feed { get; init; } = [];

    public IReadOnlyList<LiveChatMessage> Messages => [.. Feed.OfType<LiveChatMessage>()];
    public IReadOnlyList<GameEvent> Events => [.. Feed.OfType<GameEvent>()];
}
