using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinSeasonClock
{
    public static DateTime ToUtc(DateTime siteTime, string? timeZoneId)
    {
        return SiteTime.ToUtc(siteTime, SiteTime.Zone(timeZoneId));
    }
}
