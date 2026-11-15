using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityRankingViewModel
{
    public IReadOnlyList<CommunityRankingEntry> Players { get; init; } = [];
    public int MinimumComparisons { get; init; }
    public bool PlayerPages { get; init; }
    public long? Highlight { get; init; }
}
