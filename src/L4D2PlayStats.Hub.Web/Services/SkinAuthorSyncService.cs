using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Steam;
using L4D2PlayStats.Hub.Web.Models.Skins;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class SkinAuthorSyncService(
    ISkinAuthorService skinAuthorService,
    ISettingsService settingsService,
    SteamProfileClient steamProfileClient,
    IClock clock,
    ILogger<SkinAuthorSyncService> logger) : ISkinAuthorSyncService
{
    public async Task<SkinAuthorSyncResult> SyncAsync(bool everyAuthor, CancellationToken cancellationToken = default)
    {
        var now = clock.UtcNow;
        var syncedBefore = everyAuthor ? now.AddTicks(1) : now.AddHours(-AppConsts.SkinAuthorSyncIntervalInHours);
        var due = await skinAuthorService.DueAsync(syncedBefore, AppConsts.SkinAuthorsPerRun, cancellationToken);

        if (due.Count == 0)
            return new SkinAuthorSyncResult(0, 0, 0);

        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
        var lookup = await steamProfileClient.LookupAsync(due, site.SteamApiKey, cancellationToken);

        var names = lookup.Profiles.Values
            .Where(profile => !string.IsNullOrWhiteSpace(profile.Name))
            .ToDictionary(profile => profile.CommunityId, profile => profile.Name!);

        var renamed = await skinAuthorService.ApplyAsync([.. lookup.Answered], names, cancellationToken);

        if (renamed > 0)
            logger.LogInformation("Skin authors synced with Steam: {Checked} checked, {Renamed} skins renamed", lookup.Answered.Count, renamed);

        return new SkinAuthorSyncResult(due.Count, lookup.Answered.Count, renamed);
    }
}