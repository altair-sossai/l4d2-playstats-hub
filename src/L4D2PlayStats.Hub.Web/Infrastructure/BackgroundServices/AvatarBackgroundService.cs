using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using L4D2PlayStats.Hub.Web.Infrastructure.Steam;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class AvatarBackgroundService(
    IServiceProvider serviceProvider,
    SteamProfileClient steamProfileClient,
    AvatarStore avatarStore,
    AvatarQueue avatarQueue,
    IClock clock,
    ILogger<AvatarBackgroundService> logger) : BackgroundService
{
    private DateTime _nextSweepAtUtc = DateTime.MinValue;

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

    internal async Task RefreshAsync(CancellationToken cancellationToken)
    {
        if (steamProfileClient.Paused)
            return;

        var now = clock.UtcNow;

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

        var lookup = await steamProfileClient.LookupAsync(ids, key, cancellationToken);
        var client = steamProfileClient.CreateClient();

        foreach (var profile in lookup.Profiles.Values)
            if (Uri.TryCreate(profile.AvatarUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                await DownloadAsync(client, profile.CommunityId, uri, cancellationToken);

        var profiles = await dbContext.PlayerProfiles
            .Where(p => ids.Contains(p.CommunityId))
            .ToListAsync(cancellationToken);

        foreach (var profile in profiles)
            profile.AvatarUpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DownloadAsync(HttpClient client, long communityId, Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            await avatarStore.SaveAsync(communityId, await client.GetByteArrayAsync(uri, cancellationToken), cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Could not download the avatar of {CommunityId}: {Message}", communityId, exception.Message);
        }
    }
}
