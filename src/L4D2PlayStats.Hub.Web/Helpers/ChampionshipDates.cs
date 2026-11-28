using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
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

    public static DateTime? FromDateInput(string? value)
    {
        return DateTime.TryParseExact(value, AppConsts.ChampionshipDateOnlyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }

    public static string TimeInput(int? minute)
    {
        return minute == null ? string.Empty : Time(minute.Value).ToString(AppConsts.ChampionshipTimeFormat, CultureInfo.InvariantCulture);
    }

    public static int? FromTimeInput(string? value)
    {
        return TimeOnly.TryParseExact(value, AppConsts.ChampionshipTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time) ? time.Hour * AppConsts.MinutesPerHour + time.Minute : null;
    }

    public static TimeOnly Time(int minute)
    {
        return new TimeOnly(minute / AppConsts.MinutesPerHour, minute % AppConsts.MinutesPerHour);
    }

    public static string SuggestedInput(Championship championship, DateTime nowUtc, TimeZoneInfo timeZone)
    {
        return SuggestedDates.Next(championship, SiteTime.FromUtc(nowUtc, timeZone))?.ToString(AppConsts.ChampionshipLocalDateFormat, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    public static DateTime? ToUtc(string? local, TimeZoneInfo timeZone)
    {
        if (string.IsNullOrWhiteSpace(local) || !DateTime.TryParseExact(local.Trim(), AppConsts.ChampionshipLocalDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return null;

        return SiteTime.ToUtc(parsed, timeZone);
    }

    public static string Calendar(DateTime utc)
    {
        return DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString(AppConsts.ChampionshipCalendarDateFormat, CultureInfo.InvariantCulture);
    }
}
