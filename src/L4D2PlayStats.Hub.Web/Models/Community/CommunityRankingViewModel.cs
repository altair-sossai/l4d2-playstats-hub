namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityRankingViewModel
{
    public IReadOnlyList<CommunityRankingRow> Players { get; init; } = [];
    public int MinimumComparisons { get; init; }
    public int MinimumMatches { get; init; }
    public bool PlayerPages { get; init; }
}
