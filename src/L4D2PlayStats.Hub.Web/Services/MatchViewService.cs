using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Matches;

namespace L4D2PlayStats.Hub.Web.Services;

public class MatchViewService(
    IMatchResultService matchResultService,
    ISeasonService seasonService,
    IGameServerService gameServerService,
    ISettingsService settingsService,
    ICampaignImageService campaignImageService,
    ICurrentUser currentUser) : IMatchViewService
{
    public async Task<LastMatchesModel> ListAsync(int? seasonId, int? year, int? serverId, int page, CancellationToken cancellationToken = default)
    {
        if (seasonId != null)
            year = null;

        var filter = new MatchFilter { SeasonId = seasonId, Year = year, GameServerId = serverId, Page = page, PageSize = AppConsts.LastMatchesPageSize };
        var seasons = await seasonService.ListAsync(cancellationToken);
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var servers = await gameServerService.ListAsync(cancellationToken);
        var result = await matchResultService.ListAsync(filter, cancellationToken);
        var firstItem = result.Total == 0 ? 0 : (result.Page - 1) * result.PageSize + 1;
        var lastItem = Math.Min(result.Page * result.PageSize, result.Total);

        return new LastMatchesModel
        {
            Matches = await CardsAsync(result.Matches, servers, true, null, cancellationToken),
            Seasons = [.. seasons.Where(season => season.StartsAt <= now && (season.StartsAt.Year == now.Year || season.Id == seasonId))],
            Years = [.. seasons.Where(season => season.StartsAt <= now).Select(season => season.StartsAt.Year).Distinct().OrderByDescending(value => value)],
            HasPeriodChoices = seasons.Count > 1,
            Year = year,
            Url = (season, selectedYear, server) => PageUrl(null, season, selectedYear, server),
            Servers = servers,
            SeasonId = seasonId,
            ServerId = serverId,
            Pagination = new PaginationModel(result.Page, result.Pages, firstItem, lastItem, result.Total, number => PageUrl(number, seasonId, year, serverId))
        };
    }

    public async Task<IReadOnlyList<MatchCardModel>> CardsAsync(MatchFilter filter, string? playerPeriod = null, CancellationToken cancellationToken = default)
    {
        var servers = await gameServerService.ListAsync(cancellationToken);
        var result = await matchResultService.ListAsync(filter, cancellationToken);

        return await CardsAsync(result.Matches, servers, true, playerPeriod, cancellationToken);
    }

    public async Task<MatchDetailsModel?> DetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var match = await matchResultService.FindAsync(id, cancellationToken);
        if (match == null)
            return null;

        return new MatchDetailsModel
        {
            Card = await DetailsCardAsync(match, cancellationToken),
            Statistics = await matchResultService.StatisticsAsync(match, cancellationToken),
            Administrator = await currentUser.IsAdministratorAsync()
        };
    }

    public async Task<StatisticsDetailsModel?> StatisticsAsync(int id, int fileId, CancellationToken cancellationToken = default)
    {
        var match = await matchResultService.FindAsync(id, cancellationToken);
        if (match == null)
            return null;

        var statistics = await matchResultService.StatisticsAsync(match, cancellationToken);
        var statistic = statistics.FirstOrDefault(result => result.FileId == fileId);

        if (statistic == null)
            return null;

        return new StatisticsDetailsModel
        {
            Card = await DetailsCardAsync(match, cancellationToken),
            Statistics = statistics,
            Statistic = statistic
        };
    }

    private async Task<MatchCardModel> DetailsCardAsync(MatchResult match, CancellationToken cancellationToken)
    {
        var servers = await gameServerService.ListAsync(cancellationToken);
        var cards = await CardsAsync([match], servers, false, null, cancellationToken);

        return cards[0];
    }

    private async Task<IReadOnlyList<MatchCardModel>> CardsAsync(IReadOnlyList<MatchResult> matches, List<GameServer> servers, bool showDetailsButton, string? playerPeriod, CancellationToken cancellationToken)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var now = await seasonService.SiteNowAsync(cancellationToken);

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(site.TimeZone, out var timeZone))
            timeZone = TimeZoneInfo.Utc;

        return
        [
            .. matches.Select(match => new MatchCardModel
            {
                Match = match,
                TimeZone = timeZone,
                Today = now.Date,
                CampaignImage = campaignImageService.Url(match.Campaign),
                ShowDetailsButton = showDetailsButton,
                ShowServer = servers.Count > 1,
                PlayerPeriod = playerPeriod
            })
        ];
    }

    private static string PageUrl(int? page, int? seasonId, int? year, int? serverId)
    {
        var parts = new List<string>();

        if (page != null)
            parts.Add($"page={page}");

        if (seasonId != null)
            parts.Add($"season={seasonId}");

        if (year != null)
            parts.Add($"year={year}");

        if (serverId != null)
            parts.Add($"server={serverId}");

        return parts.Count == 0 ? AppConsts.MatchesUrl : $"{AppConsts.MatchesUrl}?{string.Join('&', parts)}";
    }
}
