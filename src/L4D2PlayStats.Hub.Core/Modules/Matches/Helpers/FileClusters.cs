using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;

public static class FileClusters
{
    public static (DateTime Start, DateTime End)? Around(IReadOnlyList<DateTime> orderedDates, DateTime fileDate, TimeSpan gap)
    {
        if (orderedDates.Count == 0)
            return null;

        var index = 0;

        for (var i = 0; i < orderedDates.Count; i++)
            if (orderedDates[i] <= fileDate)
                index = i;

        var start = index;
        while (start > 0 && orderedDates[start] - orderedDates[start - 1] <= gap)
            start--;

        var end = index;
        while (end < orderedDates.Count - 1 && orderedDates[end + 1] - orderedDates[end] <= gap)
            end++;

        return (orderedDates[start], orderedDates[end]);
    }

    public static List<List<StatisticsFile>> Split(IReadOnlyList<StatisticsFile> orderedFiles, TimeSpan gap)
    {
        var clusters = new List<List<StatisticsFile>>();
        List<StatisticsFile>? current = null;

        foreach (var file in orderedFiles)
        {
            if (current == null || file.FileDate - current[^1].FileDate > gap)
            {
                current = [];
                clusters.Add(current);
            }

            current.Add(file);
        }

        return clusters;
    }
}
