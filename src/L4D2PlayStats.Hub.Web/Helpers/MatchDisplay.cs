namespace L4D2PlayStats.Hub.Web.Helpers;

public static class MatchDisplay
{
    public static DateTime SiteTime(DateTime utc, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), timeZone);
    }

    public static DateTime? SiteTime(DateTime? utc, TimeZoneInfo timeZone)
    {
        return utc == null ? null : SiteTime(utc.Value, timeZone);
    }

    public static string DateAndTime(DateTime? utc, TimeZoneInfo timeZone)
    {
        var local = SiteTime(utc, timeZone);

        return local == null ? string.Empty : $"{local.Value:d} {local.Value.ToString(AppConsts.MatchTimeFormat)}";
    }

    public static string Elapsed(TimeSpan? elapsed)
    {
        return elapsed?.ToString(AppConsts.MatchElapsedFormat) ?? string.Empty;
    }
}
