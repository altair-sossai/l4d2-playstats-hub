using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;

namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class HistoryModel
{
    public RankingPeriod History { get; init; } = null!;
    public IReadOnlyList<RankingPlayer> Players { get; init; } = [];
    public IReadOnlyList<RankingPeriod> AllHistory { get; init; } = [];
}