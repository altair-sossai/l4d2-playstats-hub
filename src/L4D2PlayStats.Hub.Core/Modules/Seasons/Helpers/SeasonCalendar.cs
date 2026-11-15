namespace L4D2PlayStats.Hub.Core.Modules.Seasons.Helpers;

public static class SeasonCalendar
{
    public static int NormalizeLength(int lengthInMonths)
    {
        return Math.Clamp(lengthInMonths, 1, AppConsts.MaxSeasonLengthInMonths);
    }

    public static DateTime AlignedStart(DateTime date, int lengthInMonths)
    {
        var length = NormalizeLength(lengthInMonths);
        var block = (date.Month - 1) / length;

        return new DateTime(date.Year, 1, 1).AddMonths(block * length);
    }

    public static List<Season> Missing(IReadOnlyList<Season> existing, DateTime date, int lengthInMonths)
    {
        var length = NormalizeLength(lengthInMonths);
        var missing = new List<Season>();

        if (existing.Any(season => season.Contains(date)))
            return missing;

        if (existing.Count == 0)
        {
            var start = AlignedStart(date, length);
            missing.Add(new Season { StartsAt = start, EndsAt = start.AddMonths(length) });
            return missing;
        }

        var first = existing.MinBy(season => season.StartsAt)!;
        var last = existing.MaxBy(season => season.EndsAt)!;

        if (date >= last.EndsAt)
        {
            var start = last.EndsAt;

            while (start <= date)
            {
                missing.Add(new Season { StartsAt = start, EndsAt = start.AddMonths(length) });
                start = start.AddMonths(length);
            }

            return missing;
        }

        var end = first.StartsAt;

        while (end > date)
        {
            missing.Add(new Season { StartsAt = end.AddMonths(-length), EndsAt = end });
            end = end.AddMonths(-length);
        }

        return missing;
    }
}
