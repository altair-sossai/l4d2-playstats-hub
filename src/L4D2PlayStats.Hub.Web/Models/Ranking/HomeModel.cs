namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class HomeModel
{
    public IReadOnlyList<RankingModel> Ranking { get; init; } = [];
    public int NextResetIn { get; init; }
    public HistoryModel? LastHistory { get; init; }
}
