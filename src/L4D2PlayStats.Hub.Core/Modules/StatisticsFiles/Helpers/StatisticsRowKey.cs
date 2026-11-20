namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;

public static class StatisticsRowKey
{
    public static DateTime? ToDate(string? rowKey)
    {
        if (!long.TryParse(rowKey, out var inverted) || inverted < 0)
            return null;

        var ticks = long.MaxValue - inverted;

        return ticks > DateTime.MaxValue.Ticks ? null : new DateTime(ticks);
    }

    public static string From(DateTime fileDate)
    {
        return (long.MaxValue - fileDate.Ticks).ToString();
    }
}