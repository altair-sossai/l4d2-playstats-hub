using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Seasons;

namespace L4D2PlayStats.Hub.Web.Models;

public class PlayersViewModel
{
    public Season Season { get; init; } = null!;
    public IReadOnlyList<Season> Seasons { get; init; } = [];
    public IReadOnlyList<RankingPlayer> Players { get; init; } = [];
    public string OrderBy { get; init; } = null!;
}
