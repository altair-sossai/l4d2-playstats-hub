using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class Display
{
    public static string Percent(decimal value)
    {
        return (value * 100).ToString("0", CultureInfo.CurrentCulture) + "%";
    }

    public static string Number(decimal value)
    {
        return value.ToString("N0", CultureInfo.CurrentCulture);
    }

    public static string PerGame(int value, int games)
    {
        return games == 0 ? "-" : (value / (decimal)games).ToString("N1", CultureInfo.CurrentCulture);
    }

    public static string Duration(DateTime? start, DateTime? end)
    {
        if (start == null || end == null || end < start)
            return "-";

        var elapsed = end.Value - start.Value;

        return elapsed.TotalHours >= 1 ? $"{(int)elapsed.TotalHours}h {elapsed.Minutes:00}m" : $"{elapsed.Minutes}m";
    }

    public static string Date(DateTime utc, TimeZoneInfo timeZone)
    {
        return SiteTime.FromUtc(utc, timeZone).ToString(AppConsts.ShortDateFormat, CultureInfo.CurrentCulture);
    }

    public static string DateTime(DateTime utc, TimeZoneInfo timeZone)
    {
        return SiteTime.FromUtc(utc, timeZone).ToString(AppConsts.ShortDateTimeFormat, CultureInfo.CurrentUICulture);
    }
}
