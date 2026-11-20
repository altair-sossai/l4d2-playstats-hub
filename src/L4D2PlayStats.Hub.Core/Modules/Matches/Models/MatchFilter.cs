namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public class MatchFilter
{
    public int? SeasonId { get; set; }
    public int? Year { get; set; }
    public int? GameServerId { get; set; }
    public long? CommunityId { get; set; }
    public IReadOnlyCollection<int>? Ids { get; set; }
    public bool RankingOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
}