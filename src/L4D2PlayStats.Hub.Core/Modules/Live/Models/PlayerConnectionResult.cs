namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class PlayerConnectionResult
{
    public required string CommunityId { get; init; }
    public required string Name { get; init; }
    public required string SteamId { get; init; }
    public required string Steam3 { get; init; }
    public required string ProfileUrl { get; init; }
    public required DateTime FirstConnectedAtUtc { get; init; }
    public required DateTime LastConnectedAtUtc { get; init; }
    public required long ConnectionCount { get; init; }
}
