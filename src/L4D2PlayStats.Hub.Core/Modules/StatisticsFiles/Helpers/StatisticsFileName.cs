using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;

public static partial class StatisticsFileName
{
    public static DateTime? ToDate(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        var match = Pattern().Match(fileName);
        if (!match.Success)
            return null;

        var year = int.Parse(match.Groups[1].Value);
        var month = int.Parse(match.Groups[2].Value);
        var day = int.Parse(match.Groups[3].Value);
        var hour = int.Parse(match.Groups[4].Value);
        var minute = int.Parse(match.Groups[5].Value);
        var sequence = int.Parse(match.Groups[6].Value);

        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month) || hour > 23 || minute > 59)
            return null;

        return new DateTime(year, month, day, hour, minute, 0).AddMilliseconds(sequence);
    }

    [GeneratedRegex(@"^(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})_(\d{4})_.+\.txt$")]
    private static partial Regex Pattern();
}
