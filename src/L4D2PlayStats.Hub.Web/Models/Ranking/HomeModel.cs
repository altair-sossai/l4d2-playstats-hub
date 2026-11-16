using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class HomeModel
{
    public IReadOnlyList<RankingModel> Ranking { get; init; } = [];
    public int NextResetIn { get; init; }
    public HistoryModel? LastHistory { get; init; }
    public required PlayerPeriodModel Period { get; init; }
    public bool ShowPatent => Period.Season != null;
}
