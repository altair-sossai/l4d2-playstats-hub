using L4D2PlayStats.Hub.Core.Modules.Seasons;
using System.Globalization;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class Display
{
    public static string SeasonName(Season season)
    {
        var culture = CultureInfo.CurrentCulture;
        var last = season.EndsAt.AddDays(-1);
        var first = culture.TextInfo.ToTitleCase(season.StartsAt.ToString("MMM", culture).TrimEnd('.'));
        var final = culture.TextInfo.ToTitleCase(last.ToString("MMM", culture).TrimEnd('.'));

        if (season.StartsAt.Year == last.Year && season.StartsAt.Month == last.Month)
            return $"{first} {last.Year}";

        return season.StartsAt.Year == last.Year
            ? $"{first} – {final} {last.Year}"
            : $"{first} {season.StartsAt.Year} – {final} {last.Year}";
    }

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
}
