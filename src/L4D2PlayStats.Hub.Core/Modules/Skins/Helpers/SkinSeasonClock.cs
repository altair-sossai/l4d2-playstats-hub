namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinSeasonClock
{
    public static DateTime ToUtc(DateTime siteTime, string? timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId) || !TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZone))
            timeZone = TimeZoneInfo.Utc;

        var local = DateTime.SpecifyKind(siteTime, DateTimeKind.Unspecified);

        while (timeZone.IsInvalidTime(local))
            local = local.AddMinutes(AppConsts.SkinSeasonClockStepInMinutes);

        return TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
    }
}
