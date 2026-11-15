using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public partial class AvatarBackgroundService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    AvatarStore avatarStore,
    AvatarQueue avatarQueue,
    IClock clock,
    ILogger<AvatarBackgroundService> logger) : BackgroundService
{
    private DateTime _nextSweepAtUtc = DateTime.MinValue;
    private DateTime _pausedUntilUtc = DateTime.MinValue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not refresh the player avatars");
            }

            await avatarQueue.WaitAsync(TimeSpan.FromMinutes(AppConsts.AvatarCheckIntervalInMinutes), stoppingToken);
        }
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        if (now < _pausedUntilUtc)
            return;

        using var scope = serviceProvider.CreateScope();

        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var key = string.IsNullOrWhiteSpace(site.SteamApiKey) ? null : site.SteamApiKey.Trim();
        var limit = key == null ? AppConsts.AvatarKeylessPerRun : AppConsts.AvatarBatchSize * AppConsts.AvatarBatchesPerRun;
        var ids = avatarQueue.Take(limit).ToHashSet();

        if (now >= _nextSweepAtUtc)
        {
            var stale = now.AddHours(-AppConsts.AvatarMaxAgeInHours);
            var active = now.AddDays(-AppConsts.AvatarActivePlayerInDays);

            var outdated = await dbContext.PlayerProfiles
                .Where(p => p.LastSeenAtUtc >= active && (p.AvatarUpdatedAtUtc == null || p.AvatarUpdatedAtUtc < stale))
                .OrderBy(p => p.AvatarUpdatedAtUtc)
                .Select(p => p.CommunityId)
                .Take(Math.Max(0, limit - ids.Count))
                .ToListAsync(cancellationToken);

            ids.UnionWith(outdated);
            _nextSweepAtUtc = now.AddMinutes(AppConsts.AvatarCheckIntervalInMinutes);
        }

        if (ids.Count == 0)
            return;

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(AppConsts.SteamTimeoutInSeconds);

        var addresses = key == null
            ? await ProfileAddressesAsync(client, ids, cancellationToken)
            : await SummaryAddressesAsync(client, key, ids, cancellationToken);

        foreach (var (communityId, address) in addresses)
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                await avatarStore.SaveAsync(communityId, await client.GetByteArrayAsync(uri, cancellationToken), cancellationToken);

        var profiles = await dbContext.PlayerProfiles
            .Where(p => ids.Contains(p.CommunityId))
            .ToListAsync(cancellationToken);

        foreach (var profile in profiles)
            profile.AvatarUpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<long, string>> SummaryAddressesAsync(HttpClient client, string key, IEnumerable<long> ids, CancellationToken cancellationToken)
    {
        var addresses = new Dictionary<long, string>();

        foreach (var batch in ids.Chunk(AppConsts.AvatarBatchSize))
        {
            var url = $"{AppConsts.SteamSummariesUrl}?key={Uri.EscapeDataString(key)}&steamids={string.Join(",", batch)}";

            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                break;

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

            if (!document.RootElement.TryGetProperty("response", out var body) || !body.TryGetProperty("players", out var players))
                continue;

            foreach (var player in players.EnumerateArray())
                if (player.TryGetProperty("steamid", out var steamId) && long.TryParse(steamId.GetString(), out var communityId) && player.TryGetProperty("avatarfull", out var avatar))
                    addresses[communityId] = avatar.GetString() ?? string.Empty;
        }

        return addresses;
    }

    private async Task<Dictionary<long, string>> ProfileAddressesAsync(HttpClient client, IEnumerable<long> ids, CancellationToken cancellationToken)
    {
        var addresses = new Dictionary<long, string>();

        foreach (var batch in ids.Chunk(AppConsts.AvatarKeylessParallel))
        {
            var results = await Task.WhenAll(batch.Select(id => ProfileAddressAsync(client, id, cancellationToken)));

            foreach (var (communityId, address, limited) in results)
            {
                if (limited)
                {
                    _pausedUntilUtc = clock.UtcNow.AddMinutes(AppConsts.AvatarKeylessPauseInMinutes);

                    return addresses;
                }

                if (address != null)
                    addresses[communityId] = address;
            }

            await Task.Delay(AppConsts.AvatarKeylessDelayInMilliseconds, cancellationToken);
        }

        return addresses;
    }

    private static async Task<(long CommunityId, string? Address, bool Limited)> ProfileAddressAsync(HttpClient client, long communityId, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(string.Format(AppConsts.SteamProfileXmlUrl, communityId), cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return (communityId, null, true);

            if (!response.IsSuccessStatusCode)
                return (communityId, null, false);

            var match = AvatarFull().Match(await response.Content.ReadAsStringAsync(cancellationToken));

            return (communityId, match.Success ? match.Groups[1].Value : null, false);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return (communityId, null, false);
        }
    }

    [GeneratedRegex(AppConsts.SteamProfileAvatarPattern)]
    private static partial Regex AvatarFull();
}
