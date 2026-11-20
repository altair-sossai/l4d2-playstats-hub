using L4D2PlayStats.Hub.Core.Modules.Matches.Models;

namespace L4D2PlayStats.Hub.Web.Models.Matches;

public class MatchDetailsModel
{
    public required MatchCardModel Card { get; init; }
    public IReadOnlyList<MatchStatisticsResult> Statistics { get; init; } = [];
    public bool Administrator { get; init; }

    public MatchResult Match => Card.Match;
    public TimeZoneInfo TimeZone => Card.TimeZone;

    public string StatisticsUrl(MatchStatisticsResult statistics)
    {
        return $"{Card.DetailsUrl}/statistics/{statistics.FileId}";
    }
}