using L4D2PlayStats.Hub.Core.Modules.Matches;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Models;

public class ChampionshipPlayerStatistics
{
    public long CommunityId { get; init; }
    public int TeamId { get; init; }
    public string? Name { get; set; }
    public int Games { get; set; }
    public int Wins { get; set; }
    public MatchPlayerStatistics Totals { get; } = new();
}
