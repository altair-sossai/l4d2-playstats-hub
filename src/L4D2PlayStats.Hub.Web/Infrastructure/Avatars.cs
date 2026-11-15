using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Infrastructure;

public class AvatarStore(HubPaths paths)
{
    private readonly string _directory = paths.Avatars;

    public string PathFor(long communityId)
    {
        return Path.Combine(_directory, $"{communityId}.jpg");
    }

    public async Task SaveAsync(long communityId, byte[] image, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directory);

        await File.WriteAllBytesAsync(PathFor(communityId), image, cancellationToken);
    }
}

public class AvatarBackgroundService(
    IServiceProvider serviceProvider,
    IHttpClientFactory httpClientFactory,
    AvatarStore avatarStore,
    IClock clock,
    ILogger<AvatarBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(AppConsts.AvatarCheckIntervalInMinutes));

        do
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not refresh the player avatars");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        if (string.IsNullOrWhiteSpace(site.SteamApiKey))
            return;

        var now = clock.UtcNow;
        var stale = now.AddHours(-AppConsts.AvatarMaxAgeInHours);
        var active = now.AddDays(-AppConsts.AvatarActivePlayerInDays);

        var profiles = await dbContext.PlayerProfiles
            .Where(p => p.LastSeenAtUtc >= active && (p.AvatarUpdatedAtUtc == null || p.AvatarUpdatedAtUtc < stale))
            .OrderBy(p => p.AvatarUpdatedAtUtc)
            .Take(AppConsts.AvatarBatchSize * AppConsts.AvatarBatchesPerRun)
            .ToListAsync(cancellationToken);

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(AppConsts.SteamTimeoutInSeconds);

        foreach (var batch in profiles.Chunk(AppConsts.AvatarBatchSize))
        {
            var ids = string.Join(",", batch.Select(p => p.CommunityId));
            var url = $"{AppConsts.SteamSummariesUrl}?key={Uri.EscapeDataString(site.SteamApiKey.Trim())}&steamids={ids}";

            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return;

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

            var avatars = new Dictionary<long, string>();

            if (document.RootElement.TryGetProperty("response", out var body) && body.TryGetProperty("players", out var players))
                foreach (var player in players.EnumerateArray())
                    if (player.TryGetProperty("steamid", out var steamId) && long.TryParse(steamId.GetString(), out var communityId) && player.TryGetProperty("avatarfull", out var avatar))
                        avatars[communityId] = avatar.GetString() ?? string.Empty;

            foreach (var profile in batch)
            {
                if (avatars.TryGetValue(profile.CommunityId, out var avatarUrl) && Uri.TryCreate(avatarUrl, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                    await avatarStore.SaveAsync(profile.CommunityId, await client.GetByteArrayAsync(uri, cancellationToken), cancellationToken);

                profile.AvatarUpdatedAtUtc = now;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
