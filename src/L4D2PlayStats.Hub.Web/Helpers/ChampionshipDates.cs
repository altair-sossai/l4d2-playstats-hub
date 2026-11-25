using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ChampionshipDates
{
    public static string Input(DateTime? utc, TimeZoneInfo timeZone)
    {
        return utc == null ? string.Empty : SiteTime.FromUtc(utc.Value, timeZone).ToString(AppConsts.ChampionshipLocalDateFormat, CultureInfo.InvariantCulture);
    }

    public static string DateInput(DateTime? date)
    {
        return date?.ToString(AppConsts.ChampionshipDateOnlyFormat, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    public static DateTime? ToUtc(string? local, TimeZoneInfo timeZone)
    {
        if (string.IsNullOrWhiteSpace(local) || !DateTime.TryParseExact(local.Trim(), AppConsts.ChampionshipLocalDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return null;

        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified), timeZone);
    }

    public static string Calendar(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString(AppConsts.ChampionshipCalendarDateFormat, CultureInfo.InvariantCulture);
    }
}
