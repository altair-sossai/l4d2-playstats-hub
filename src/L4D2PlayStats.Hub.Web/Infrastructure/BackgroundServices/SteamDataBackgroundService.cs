using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Steam;

namespace L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;

public class SteamDataBackgroundService(
    IServiceProvider serviceProvider,
    SteamWebApiClient steamWebApiClient,
    ILogger<SteamDataBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(AppConsts.SteamDataStartDelayInSeconds), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RefreshAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not refresh the Steam data of the players");
            }

            await Task.Delay(TimeSpan.FromMinutes(AppConsts.SteamDataIntervalInMinutes), stoppingToken);
        }
    }

    internal async Task<int> RefreshAsync(CancellationToken cancellationToken)
    {
        if (steamWebApiClient.Paused)
            return 0;

        using var scope = serviceProvider.CreateScope();

        var site = await scope.ServiceProvider.GetRequiredService<ISettingsService>().GetAsync<SiteSettings>(cancellationToken);
        var key = string.IsNullOrWhiteSpace(site.SteamApiKey) ? null : site.SteamApiKey.Trim();

        if (key == null)
            return 0;

        var steamService = scope.ServiceProvider.GetRequiredService<IPlayerSteamService>();
        var due = await steamService.DueAsync(AppConsts.SteamDataPlayersPerRun, cancellationToken);
        var updates = new List<PlayerSteamUpdate>();

        foreach (var batch in due.Chunk(AppConsts.SteamDataBatchSize))
            updates.AddRange(await BatchAsync(batch, key, cancellationToken));

        await steamService.ApplyAsync(updates, cancellationToken);

        return updates.Count(update => update.Answered);
    }

    private async Task<List<PlayerSteamUpdate>> BatchAsync(long[] ids, string key, CancellationToken cancellationToken)
    {
        var summaries = await steamWebApiClient.SummariesAsync(ids, key, cancellationToken);
        var bans = await steamWebApiClient.BansAsync(ids, key, cancellationToken);
        var updates = new List<PlayerSteamUpdate>();

        foreach (var communityId in ids)
        {
            var update = new PlayerSteamUpdate(communityId);
            var summary = summaries.Value.GetValueOrDefault(communityId);
            var ban = bans.Value.GetValueOrDefault(communityId);

            if (summaries.Ok)
            {
                update.HasSummary = true;
                update.SteamName = summary?.Name;
                update.AccountCreatedAtUtc = summary?.CreatedAtUtc;
            }

            if (bans.Ok && ban != null)
            {
                update.HasBans = true;
                update.VacBans = ban.VacBans;
                update.GameBans = ban.GameBans;
                update.DaysSinceLastBan = ban.DaysSinceLastBan;
            }

            if (summaries.Ok || bans.Ok)
                await DetailsAsync(update, key, cancellationToken);

            updates.Add(update);
        }

        return updates;
    }

    private async Task DetailsAsync(PlayerSteamUpdate update, string key, CancellationToken cancellationToken)
    {
        var playtime = await steamWebApiClient.PlaytimeAsync(update.CommunityId, key, cancellationToken);

        if (playtime.Ok)
        {
            update.HasPlaytime = true;
            update.Left4Dead2Minutes = playtime.Value?.TotalMinutes;
            update.Left4Dead2RecentMinutes = playtime.Value?.RecentMinutes;
        }

        var level = await steamWebApiClient.LevelAsync(update.CommunityId, key, cancellationToken);

        if (!level.Ok)
            return;

        update.HasLevel = true;
        update.SteamLevel = level.Value;
    }
}
