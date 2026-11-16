namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public record MatchResultPage(IReadOnlyList<MatchResult> Matches, int Total, int Page, int PageSize)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}
