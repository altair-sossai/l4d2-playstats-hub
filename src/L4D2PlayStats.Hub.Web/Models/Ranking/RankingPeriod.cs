using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Models.Ranking;

public class RankingPeriod
{
    private RankingPeriod()
    {
    }

    public RankingPeriodKind Kind { get; private init; }
    public string Id { get; private init; } = null!;
    public Season? Season { get; private init; }
    public int StartYear { get; private init; }
    public int StartMonth { get; private init; }
    public int EndYear { get; private init; }
    public int EndMonth { get; private init; }
    public bool IsSeason => Kind == RankingPeriodKind.Season;
    public bool IsAnnual => Kind == RankingPeriodKind.Year;
    public bool IsAllTime => Kind == RankingPeriodKind.AllTime;

    public static RankingPeriod AllTime()
    {
        return new RankingPeriod { Kind = RankingPeriodKind.AllTime, Id = AppConsts.AllTimeHistoryId };
    }

    public static RankingPeriod ForYear(int year)
    {
        return new RankingPeriod { Kind = RankingPeriodKind.Year, Id = year.ToString(), StartYear = year, StartMonth = 1, EndYear = year, EndMonth = 12 };
    }

    public static RankingPeriod ForSeason(Season season)
    {
        var last = season.EndsAt.AddDays(-1);

        return new RankingPeriod
        {
            Kind = RankingPeriodKind.Season,
            Id = $"{season.StartsAt:yyyy-MM}",
            Season = season,
            StartYear = season.StartsAt.Year,
            StartMonth = season.StartsAt.Month,
            EndYear = last.Year,
            EndMonth = last.Month
        };
    }

    public string Label(IStringLocalizer<SharedResource> localizer)
    {
        return Kind switch
        {
            RankingPeriodKind.Season => localizer["From {1:00}/{0:0000} to {3:00}/{2:0000}", StartYear, StartMonth, EndYear, EndMonth],
            RankingPeriodKind.Year => localizer["Year of {0}", StartYear],
            _ => localizer["All time"]
        };
    }
}
