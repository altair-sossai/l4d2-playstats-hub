using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinSeasonClock
{
    public static DateTime ToUtc(DateTime siteTime, string? timeZoneId)
    {
        var timeZone = SiteTime.Zone(timeZoneId);

        var local = DateTime.SpecifyKind(siteTime, DateTimeKind.Unspecified);

        while (timeZone.IsInvalidTime(local))
            local = local.AddMinutes(AppConsts.SkinSeasonClockStepInMinutes);

        return TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
    }
}
