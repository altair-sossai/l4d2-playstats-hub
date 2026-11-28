namespace L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

public static class SiteTime
{
    public static TimeZoneInfo Zone(string? timeZoneId)
    {
        return !string.IsNullOrEmpty(timeZoneId) && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var timeZone) ? timeZone : TimeZoneInfo.Utc;
    }

    public static DateTime FromUtc(DateTime utc, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);
    }

    public static DateTime ToUtc(DateTime siteTime, TimeZoneInfo timeZone)
    {
        var local = DateTime.SpecifyKind(siteTime, DateTimeKind.Unspecified);

        while (timeZone.IsInvalidTime(local))
            local = local.AddMinutes(AppConsts.SiteTimeGapStepInMinutes);

        return TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
    }
}
