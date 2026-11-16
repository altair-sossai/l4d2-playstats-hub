using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class PlayerConnectionsViewModel
{
    public string? Query { get; init; }
    public long? CommunityId { get; init; }
    public PlayerConnectionDetails? Details { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
    public bool RankingEnabled { get; init; }
}
