using L4D2PlayStats.Hub.Core.Modules.Community.Settings;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityRankingViewModel
{
    public required PagedListModel<CommunityRankingRow> Players { get; init; }
    public int MinimumComparisons { get; init; }
    public int MinimumMatches { get; init; }
    public CommunityRankingSettings Settings { get; init; } = new();
    public bool Patents { get; init; }
}
