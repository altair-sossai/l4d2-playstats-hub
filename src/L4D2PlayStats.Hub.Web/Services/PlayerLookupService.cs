using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using L4D2PlayStats.Hub.Web.Infrastructure.Steam;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Web.Services;

public class PlayerLookupService(
    IRankingService rankingService,
    IPlayerNameService playerNameService,
    ISettingsService settingsService,
    SteamProfileClient steamProfileClient,
    AvatarStore avatarStore,
    IMemoryCache memoryCache) : IPlayerLookupService
{
    public async Task<PlayerLookupResult?> LookupAsync(string? value, CancellationToken cancellationToken = default)
    {
        var parsed = SteamIdParser.Parse(value);
        if (parsed == null)
            return null;

        var communityId = parsed.Value;
        var siteName = await SiteNameAsync(communityId, cancellationToken);
        var steam = siteName == null ? await SteamProfileAsync(communityId, cancellationToken) : null;
        var stored = File.Exists(avatarStore.PathFor(communityId));

        var name = siteName ?? (string.IsNullOrWhiteSpace(steam?.Name) ? null : steam.Name);
        var avatarUrl = !stored && SafeUrl.IsHttps(steam?.AvatarUrl) ? steam!.AvatarUrl : null;

        return new PlayerLookupResult(communityId, name, avatarUrl, siteName != null);
    }

    private async Task<string?> SiteNameAsync(long communityId, CancellationToken cancellationToken)
    {
        var current = await rankingService.CurrentAsync(cancellationToken);
        var player = current.FirstOrDefault(item => item.CommunityId == communityId);

        if (player == null)
        {
            var allTime = await rankingService.AllTimeAsync(cancellationToken);
            player = allTime.FirstOrDefault(item => item.CommunityId == communityId);
        }

        if (!string.IsNullOrWhiteSpace(player?.MainName))
            return player.MainName;

        return await playerNameService.KnownNameAsync(communityId, cancellationToken);
    }

    private async Task<SteamProfile?> SteamProfileAsync(long communityId, CancellationToken cancellationToken)
    {
        return await memoryCache.GetOrCreateAsync(string.Format(AppConsts.PlayerLookupSteamNameCacheKey, communityId), async entry =>
        {
            var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
            var lookup = await steamProfileClient.LookupAsync([communityId], site.SteamApiKey, cancellationToken);
            var answered = lookup.Answered.Contains(communityId);

            entry.AbsoluteExpirationRelativeToNow = answered
                ? TimeSpan.FromHours(AppConsts.PlayerLookupSteamNameCacheInHours)
                : TimeSpan.FromMinutes(AppConsts.PlayerLookupFailureCacheInMinutes);

            return lookup.Profiles.GetValueOrDefault(communityId);
        });
    }
}
