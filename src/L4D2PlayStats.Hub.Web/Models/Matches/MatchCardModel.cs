using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Matches;

public class MatchCardModel
{
    public required MatchResult Match { get; init; }
    public required TimeZoneInfo TimeZone { get; init; }
    public required DateTime Today { get; init; }
    public required string CampaignImage { get; init; }
    public bool ShowDetailsButton { get; init; } = true;
    public bool ShowServer { get; init; }
    public string? PlayerPeriod { get; init; }

    public DateTime MatchStart => MatchDisplay.SiteTime(Match.MatchStart, TimeZone);
    public string DetailsUrl => $"{AppConsts.MatchesUrl}/{Match.Id}";

    public string PlayerUrl(long communityId)
    {
        return PlayerUrls.Details(communityId, null, PlayerPeriod);
    }
}
