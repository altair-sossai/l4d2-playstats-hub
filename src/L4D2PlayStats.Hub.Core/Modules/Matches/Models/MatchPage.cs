namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public record MatchPage(IReadOnlyList<Match> Matches, int Total, int Page, int PageSize)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
}