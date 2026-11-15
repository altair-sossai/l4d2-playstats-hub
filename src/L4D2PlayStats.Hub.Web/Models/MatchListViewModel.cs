using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models;

public class MatchListViewModel
{
    public MatchPage Page { get; init; } = null!;
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public int? SeasonId { get; init; }
    public int? ServerId { get; init; }
}
