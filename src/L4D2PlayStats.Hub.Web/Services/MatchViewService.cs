using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Matches;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class MatchViewService(
    IMatchResultService matchResultService,
    ISeasonService seasonService,
    IGameServerService gameServerService,
    ISettingsService settingsService,
    ISiteClock siteClock,
    ICampaignImageService campaignImageService,
    ICurrentUser currentUser) : IMatchViewService
{
    public async Task<LastMatchesModel> ListAsync(int? seasonId, int? year, int? serverId, int page, CancellationToken cancellationToken = default)
    {
        if (seasonId != null)
            year = null;

        var ranking = await settingsService.GetAsync<RankingSettings>(cancellationToken);
        var filter = new MatchFilter { SeasonId = seasonId, Year = year, GameServerId = serverId, Page = page, PageSize = ranking.MatchesPerPage };
        var seasons = await seasonService.ListAsync(cancellationToken);
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var servers = await gameServerService.ListAsync(cancellationToken);
        var result = await matchResultService.ListAsync(filter, cancellationToken);

        return new LastMatchesModel
        {
            Matches = await CardsAsync(result.Items, servers, true, null, cancellationToken),
            Seasons = [.. seasons.Where(season => season.StartsAt <= now && (season.StartsAt.Year == now.Year || season.Id == seasonId))],
            Years = [.. seasons.Where(season => season.StartsAt <= now).Select(season => season.StartsAt.Year).Distinct().OrderByDescending(value => value)],
            HasPeriodChoices = seasons.Count > 1,
            Year = year,
            Url = (season, selectedYear, server) => PageUrl(null, season, selectedYear, server),
            Servers = servers,
            SeasonId = seasonId,
            ServerId = serverId,
            Pagination = PaginationModel.Of(result, number => PageUrl(number, seasonId, year, serverId))
        };
    }

    public async Task<IReadOnlyList<MatchCardModel>> CardsAsync(MatchFilter filter, string? playerPeriod = null, CancellationToken cancellationToken = default)
    {
        var servers = await gameServerService.ListAsync(cancellationToken);
        var result = await matchResultService.ListAsync(filter, cancellationToken);

        return await CardsAsync(result.Items, servers, true, playerPeriod, cancellationToken);
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
            Statistic = statistic,
            Administrator = await currentUser.IsAdministratorAsync()
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
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);

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
        return QueryUrl.Build(AppConsts.MatchesUrl, (AppConsts.AdminPageQuery, page), (AppConsts.MatchSeasonQuery, seasonId), (AppConsts.MatchYearQuery, year), (AppConsts.MatchServerQuery, serverId));
    }
}
