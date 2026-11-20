using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public class SkinSeasonRefundService(
    ISettingsService settingsService,
    ISeasonService seasonService,
    ISkinShopService skinShopService) : ISkinSeasonRefundService
{
    public async Task<SkinRefundSummary> RunAsync(CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        if (!modules.IsEnabled(HubModule.SkinShop))
            return new SkinRefundSummary();

        var season = await seasonService.GetCurrentAsync(cancellationToken);
        var state = await settingsService.GetAsync<SkinSeasonRefundState>(cancellationToken);

        if (state.SeasonId == season.Id)
            return new SkinRefundSummary();

        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var summary = new SkinRefundSummary();

        if (settings.RefundAtSeasonEnd)
        {
            var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);
            var seasonStart = SkinSeasonClock.ToUtc(season.StartsAt, site.TimeZone);

            summary = await skinShopService.RefundPurchasesBeforeAsync(seasonStart, cancellationToken);
        }

        state.SeasonId = season.Id;
        await settingsService.SaveAsync(state, cancellationToken);

        return summary;
    }
}
