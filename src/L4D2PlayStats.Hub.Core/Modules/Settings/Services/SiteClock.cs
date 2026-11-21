using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Services;

public class SiteClock(ISettingsService settingsService) : ISiteClock
{
    public async Task<TimeZoneInfo> ZoneAsync(CancellationToken cancellationToken = default)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return SiteTime.Zone(site.TimeZone);
    }

    public async Task<DateTime> ToLocalAsync(DateTime utc, CancellationToken cancellationToken = default)
    {
        return SiteTime.FromUtc(utc, await ZoneAsync(cancellationToken));
    }
}
