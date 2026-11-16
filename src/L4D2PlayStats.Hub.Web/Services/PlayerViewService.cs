using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Players.Services;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Services;

public class PlayerViewService(
    HubDbContext dbContext,
    ISettingsService settingsService,
    ISeasonService seasonService,
    IRankingService rankingService,
    IPlayerMatchService playerMatchService,
    IPlayerConnectionService playerConnectionService,
    ISkinService skinService,
    ISkinShopService skinShopService,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer) : IPlayerViewService
{
    public async Task<PlayerPeriodModel?> PeriodAsync(int? seasonId, CancellationToken cancellationToken = default)
    {
        var current = await seasonService.GetCurrentAsync(cancellationToken);
        var seasons = await seasonService.ListAsync(cancellationToken);
        var started = seasons.Where(season => season.StartsAt <= current.StartsAt).ToList();
        var selected = seasonId == null ? current : started.FirstOrDefault(season => season.Id == seasonId);

        return selected == null ? null : new PlayerPeriodModel { Season = selected, CurrentSeason = current, Seasons = started };
    }

    public async Task<PlayersModel> ListAsync(PlayerPeriodModel period, PlayerResultProperty orderBy, bool asc, CancellationToken cancellationToken = default)
    {
        var players = await rankingService.SeasonAsync(period.Season.Id, cancellationToken);

        return new PlayersModel(PlayerProperties.Sort(players, orderBy, asc), orderBy, asc, period);
    }

    public async Task<PlayerDetailsModel?> DetailsAsync(PlayerPeriodModel period, long communityId, long? compareWith, CancellationToken cancellationToken = default)
    {
        var players = await rankingService.SeasonAsync(period.Season.Id, cancellationToken);
        var firstPlayer = players.FirstOrDefault(player => player.CommunityId == communityId);
        var secondPlayer = compareWith == null ? null : players.FirstOrDefault(player => player.CommunityId == compareWith);

        if (firstPlayer == null)
            return null;

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var today = await seasonService.SiteNowAsync(cancellationToken);
        var skinShop = modules.IsEnabled(HubModule.SkinShop);

        long[] communityIds = secondPlayer == null ? [firstPlayer.CommunityId] : [firstPlayer.CommunityId, secondPlayer.CommunityId];

        var matches = await playerMatchService.LastAsync(period.Season.Id, communityIds, AppConsts.PlayerRecentMatches, cancellationToken);

        var relatedPlayers = modules.IsEnabled(HubModule.LiveServer)
            ? await playerConnectionService.RelatedAsync(communityId, cancellationToken)
            : [];

        var firstPlayerRanking = new RankingModel(localizer, firstPlayer);
        var secondPlayerRanking = secondPlayer == null ? null : new RankingModel(localizer, secondPlayer);
        var knownNames = await KnownNamesAsync(players, firstPlayer, secondPlayer, cancellationToken);
        var matchModels = matches.Select(match => new PlayerMatchModel(match, today) { SeasonId = period.SeasonId }).ToList();

        IReadOnlyCollection<Skin> skins = skinShop ? await skinService.ListAsync(cancellationToken) : [];
        var assetsUrl = skinShop ? (await settingsService.GetAsync<SkinShopSettings>(cancellationToken)).AssetsUrl : null;

        return new PlayerDetailsModel(firstPlayerRanking, secondPlayerRanking, players, matchModels, relatedPlayers, knownNames, period)
        {
            SkinShop = skinShop,
            FirstPlayerSkins = skinShop ? await PlayerSkinsAsync(skins, firstPlayer.CommunityId, assetsUrl, cancellationToken) : null,
            SecondPlayerSkins = skinShop && secondPlayer != null ? await PlayerSkinsAsync(skins, secondPlayer.CommunityId, assetsUrl, cancellationToken) : null,
            Administrator = await currentUser.IsAdministratorAsync(),
            TimeZone = TimeZone(site)
        };
    }

    public async Task<PlayerNotFoundModel> NotFoundAsync(PlayerPeriodModel period, long communityId, CancellationToken cancellationToken = default)
    {
        if (communityId <= 0)
            return new PlayerNotFoundModel(communityId, null, []) { Period = period };

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        var connectionInfo = modules.IsEnabled(HubModule.LiveServer)
            ? await playerConnectionService.DetailsAsync(communityId, cancellationToken)
            : new PlayerConnectionDetails(null, []);

        var knownName = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(profile => profile.CommunityId == communityId)
            .Select(profile => profile.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return new PlayerNotFoundModel(communityId, connectionInfo.Player, connectionInfo.RelatedPlayers, knownName)
        {
            Period = period,
            TimeZone = TimeZone(site)
        };
    }

    private async Task<PlayerSkinsModel> PlayerSkinsAsync(IReadOnlyCollection<Skin> skins, long communityId, string? assetsUrl, CancellationToken cancellationToken)
    {
        var playerSkins = await skinShopService.GetPlayerSkinsAsync(communityId, cancellationToken);
        var wallet = await skinShopService.GetWalletAsync(communityId, cancellationToken);

        return new PlayerSkinsModel(skins, playerSkins, wallet, assetsUrl);
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

        var profiles = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(profile => unranked.Contains(profile.CommunityId) && profile.Name != null)
            .Select(profile => new { profile.CommunityId, profile.Name })
            .ToListAsync(cancellationToken);

        return profiles.ToDictionary(profile => profile.CommunityId, profile => profile.Name!);
    }

    private static TimeZoneInfo TimeZone(SiteSettings site)
    {
        return TimeZoneInfo.TryFindSystemTimeZoneById(site.TimeZone, out var timeZone) ? timeZone : TimeZoneInfo.Utc;
    }
}
