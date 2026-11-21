using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class PlayerConnectionResult(long communityId) : SteamIdentity(communityId)
{
    public required string Name { get; init; }
    public required DateTime FirstConnectedAtUtc { get; init; }
    public required DateTime LastConnectedAtUtc { get; init; }
    public required long ConnectionCount { get; init; }
}
