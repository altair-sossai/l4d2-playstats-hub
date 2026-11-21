using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Matches.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Infrastructure.Skins;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Matches;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Services;

public class PlayerViewService(
    ISettingsService settingsService,
    ISiteClock siteClock,
    ISeasonService seasonService,
    IRankingService rankingService,
    IPlayerMatchService playerMatchService,
    IMatchViewService matchViewService,
    IPlayerConnectionService playerConnectionService,
    IPlayerNameService playerNameService,
    IPlayerSteamService playerSteamService,
    IClock clock,
    ISkinService skinService,
    ISkinShopService skinShopService,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer,
    SkinThumbnailStore skinThumbnailStore) : IPlayerViewService
{
    public async Task<PlayerPeriodModel?> PeriodAsync(int? seasonId, int? year, bool allTime, CancellationToken cancellationToken = default)
    {
        var current = await seasonService.GetCurrentAsync(cancellationToken);
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var seasons = await seasonService.ListAsync(cancellationToken);
        var started = seasons.Where(season => season.StartsAt <= current.StartsAt).ToList();
        var years = started.Select(season => season.StartsAt.Year).Distinct().OrderByDescending(value => value).ToList();

        var options = started
            .Where(season => season.StartsAt.Year == now.Year || season.Id == current.Id)
            .Select(season => new PlayerPeriodOption { Season = season, Query = season.Id == current.Id ? null : PlayerUrls.SeasonQuery(season.Id) })
            .Concat(years.Select(value => new PlayerPeriodOption { Year = value, Query = PlayerUrls.YearQuery(value) }))
            .Append(new PlayerPeriodOption { AllTime = true, Query = PlayerUrls.AllTimeQuery() })
            .ToList();

        if (allTime)
            return new PlayerPeriodModel { AllTime = true, CurrentSeason = current, Options = options };

        if (year != null)
            return years.Contains(year.Value) ? new PlayerPeriodModel { Year = year, CurrentSeason = current, Options = options } : null;

        var selected = seasonId == null ? current : started.FirstOrDefault(season => season.Id == seasonId);

        return selected == null ? null : new PlayerPeriodModel { Season = selected, CurrentSeason = current, Options = options };
    }

    public Task<IReadOnlyList<RankingPlayer>> RankingAsync(PlayerPeriodModel period, CancellationToken cancellationToken = default)
    {
        if (period.AllTime)
            return rankingService.AllTimeAsync(cancellationToken);

        return period.Year != null
            ? rankingService.YearAsync(period.Year.Value, cancellationToken)
            : rankingService.SeasonAsync(period.Season!.Id, cancellationToken);
    }

    public async Task<PlayersModel> ListAsync(PlayerPeriodModel period, PlayerResultProperty orderBy, bool asc, int page, CancellationToken cancellationToken = default)
    {
        var players = await RankingAsync(period, cancellationToken);

        var sorted = PlayerProperties.Sort(players, orderBy, asc);

        return new PlayersModel(new PagedListModel<PlayerListRow>(sorted, page, AppConsts.PlayersPageSize), orderBy, asc, period);
    }

    public async Task<PlayerDetailsModel?> DetailsAsync(PlayerPeriodModel period, long communityId, long? compareWith, CancellationToken cancellationToken = default)
    {
        var players = await RankingAsync(period, cancellationToken);
        var firstPlayer = players.FirstOrDefault(player => player.CommunityId == communityId);
        var secondPlayer = compareWith == null ? null : players.FirstOrDefault(player => player.CommunityId == compareWith);

        if (firstPlayer == null)
            return null;

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var skinShop = modules.IsEnabled(HubModule.SkinShop);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);

        long[] communityIds = secondPlayer == null ? [firstPlayer.CommunityId] : [firstPlayer.CommunityId, secondPlayer.CommunityId];

        var matches = await MatchesAsync(period, communityIds, cancellationToken);

        var relatedPlayers = modules.IsEnabled(HubModule.LiveServer) && await RelatedAccountsVisibleAsync(cancellationToken)
            ? await playerConnectionService.RelatedAsync(communityId, cancellationToken)
            : [];

        var firstPlayerRanking = new RankingModel(localizer, firstPlayer);
        var secondPlayerRanking = secondPlayer == null ? null : new RankingModel(localizer, secondPlayer);
        var knownNames = await KnownNamesAsync(players, firstPlayer, secondPlayer, cancellationToken);
        var steam = await playerSteamService.DetailsAsync(communityIds, cancellationToken);

        IReadOnlyCollection<Skin> skins = skinShop ? await skinService.ListAsync(cancellationToken) : [];
        var assetsUrl = skinShop ? (await settingsService.GetAsync<SkinShopSettings>(cancellationToken)).AssetsUrl : null;

        return new PlayerDetailsModel(firstPlayerRanking, secondPlayerRanking, players, matches, relatedPlayers, knownNames, period)
        {
            SkinShop = skinShop,
            FirstPlayerSkins = skinShop ? await PlayerSkinsAsync(skins, firstPlayer.CommunityId, assetsUrl, cancellationToken) : null,
            SecondPlayerSkins = skinShop && secondPlayer != null ? await PlayerSkinsAsync(skins, secondPlayer.CommunityId, assetsUrl, cancellationToken) : null,
            FirstPlayerSteam = new PlayerSteamModel(steam.GetValueOrDefault(firstPlayer.CommunityId), timeZone, clock.UtcNow),
            SecondPlayerSteam = secondPlayer == null ? null : new PlayerSteamModel(steam.GetValueOrDefault(secondPlayer.CommunityId), timeZone, clock.UtcNow),
            Administrator = period.Season != null && await currentUser.IsAdministratorAsync(),
            TimeZone = timeZone
        };
    }

    public async Task<PlayerNotFoundModel> NotFoundAsync(PlayerPeriodModel? period, long communityId, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        var connectionInfo = modules.IsEnabled(HubModule.LiveServer)
            ? await playerConnectionService.DetailsAsync(communityId, cancellationToken)
            : new PlayerConnectionDetails(null, []);
        var relatedPlayers = await RelatedAccountsVisibleAsync(cancellationToken) ? connectionInfo.RelatedPlayers : [];
        var knownName = await playerNameService.KnownNameAsync(communityId, cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);
        var steam = await playerSteamService.DetailsAsync([communityId], cancellationToken);

        return new PlayerNotFoundModel(communityId, connectionInfo.Player, relatedPlayers, knownName)
        {
            Period = period,
            TimeZone = timeZone,
            Steam = new PlayerSteamModel(steam.GetValueOrDefault(communityId), timeZone, clock.UtcNow)
        };
    }

    private async Task<IReadOnlyList<MatchCardModel>> MatchesAsync(PlayerPeriodModel period, long[] communityIds, CancellationToken cancellationToken)
    {
        var ranking = await settingsService.GetAsync<RankingSettings>(cancellationToken);
        var ids = await playerMatchService.RecentMatchIdsAsync(period.Season?.Id, period.Year, communityIds, ranking.PlayerRecentMatches, cancellationToken);

        if (ids.Count == 0)
            return [];

        return await matchViewService.CardsAsync(new MatchFilter { Ids = ids, PageSize = ids.Count }, period.Query, cancellationToken);
    }

    private async Task<bool> RelatedAccountsVisibleAsync(CancellationToken cancellationToken)
    {
        var privacy = await settingsService.GetAsync<PrivacySettings>(cancellationToken);

        return privacy.RelatedAccounts == RelatedAccountsVisibility.Everyone || await currentUser.IsAdministratorAsync();
    }

    private async Task<PlayerSkinsModel> PlayerSkinsAsync(IReadOnlyCollection<Skin> skins, long communityId, string? assetsUrl, CancellationToken cancellationToken)
    {
        var playerSkins = await skinShopService.GetPlayerSkinsAsync(communityId, cancellationToken);
        var wallet = await skinShopService.GetWalletAsync(communityId, cancellationToken);

        return new PlayerSkinsModel(skins, playerSkins, wallet, assetsUrl, skinThumbnailStore);
    }

    private async Task<Dictionary<long, string>> KnownNamesAsync(IReadOnlyList<RankingPlayer> players, RankingPlayer firstPlayer, RankingPlayer? secondPlayer, CancellationToken cancellationToken)
    {
        var ranked = players.Select(player => player.CommunityId).ToHashSet();

        var unranked = firstPlayer.Relations.Keys
            .Concat(secondPlayer?.Relations.Keys ?? Enumerable.Empty<long>())
            .Where(communityId => !ranked.Contains(communityId))
            .Distinct()
            .ToList();

        if (unranked.Count == 0)
            return [];

        return await playerNameService.NamesAsync(unranked, cancellationToken);
    }
}
