using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class PlayerUrls
{
    public static string Details(long communityId, long? compareWith = null, string? period = null)
    {
        var path = compareWith == null
            ? $"{AppConsts.PlayerUrl}/{communityId}"
            : $"{AppConsts.PlayerUrl}/{communityId}/{compareWith}";

        return path + Period(period, '?');
    }

    public static string List(PlayerResultProperty? orderBy = null, bool? asc = null, string? period = null)
    {
        if (orderBy == null || asc == null)
            return AppConsts.PlayersUrl + Period(period, '?');

        return $"{AppConsts.PlayersUrl}?orderBy={orderBy}&asc={asc}" + Period(period, '&');
    }

    public static string List(PlayerResultProperty orderBy, bool asc, string? period, int page)
    {
        return $"{List(orderBy, asc, period)}&page={page}";
    }

    public static string Summary(PlayerResultProperty? sort, bool asc, string? period, int? page = null)
    {
        var query = new List<string>();

        if (sort != null)
            query.Add($"{AppConsts.PlayerSortParameter}={sort}");

        if (asc)
            query.Add("asc=True");

        if (!string.IsNullOrEmpty(period))
            query.Add(period);

        if (page is > 1)
            query.Add($"page={page}");

        return query.Count == 0 ? AppConsts.PlayersUrl : $"{AppConsts.PlayersUrl}?{string.Join('&', query)}";
    }

    public static string Avatar(long communityId)
    {
        return $"{AppConsts.AvatarUrl}{communityId}";
    }

    public static string SeasonQuery(int seasonId)
    {
        return $"{AppConsts.PlayerSeasonParameter}={seasonId}";
    }

    public static string AllTimeQuery()
    {
        return $"{AppConsts.PlayerPeriodParameter}={AppConsts.AllTimeHistoryId}";
    }

    public static string YearQuery(int year)
    {
        return $"{AppConsts.PlayerYearParameter}={year}";
    }

    private static string Period(string? period, char separator)
    {
        return string.IsNullOrEmpty(period) ? string.Empty : $"{separator}{period}";
    }
}
