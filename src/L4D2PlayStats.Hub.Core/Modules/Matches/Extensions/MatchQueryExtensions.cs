namespace L4D2PlayStats.Hub.Core.Modules.Matches.Extensions;

public static class MatchQueryExtensions
{
    public static IQueryable<Match> InPeriod(this IQueryable<Match> query, int? seasonId, int? year)
    {
        if (seasonId != null)
            query = query.Where(m => m.SeasonId == seasonId);

        return year == null ? query : query.InYear(year.Value);
    }

    public static IQueryable<Match> InYear(this IQueryable<Match> query, int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = start.AddYears(1);

        return query.Where(m => m.LastFileDate >= start && m.LastFileDate < end);
    }

    public static IQueryable<Match> NewestFirst(this IQueryable<Match> query)
    {
        return query.OrderByDescending(m => m.LastFileDate).ThenByDescending(m => m.Id);
    }
}