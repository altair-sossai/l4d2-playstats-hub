using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Seasons;

namespace L4D2PlayStats.Hub.Web.Models;

public class InGameViewModel
{
    public string SiteName { get; init; } = null!;
    public Season Season { get; init; } = null!;
    public IReadOnlyList<RankingRow> Players { get; init; } = [];
    public RankingRow? Me { get; init; }
    public IReadOnlyList<Match> Matches { get; init; } = [];
    public long? SteamId { get; init; }
}