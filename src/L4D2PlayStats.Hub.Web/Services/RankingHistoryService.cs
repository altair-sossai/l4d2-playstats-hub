using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class RankingHistoryService(ISeasonService seasonService, IRankingService rankingService) : IRankingHistoryService
{
    public async Task<IReadOnlyList<RankingPeriod>> ListAsync(CancellationToken cancellationToken = default)
    {
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var seasons = await seasonService.ListAsync(cancellationToken);
        var finished = seasons.Where(season => season.EndsAt <= now).OrderBy(season => season.StartsAt).ToList();
        var periods = new List<RankingPeriod>();

        if (seasons.Count == 0)
            return periods;

        periods.Add(RankingPeriod.AllTime());

        foreach (var year in seasons.Select(season => season.StartsAt.Year).Distinct().Order())
        {
            if (year == now.Year)
                periods.AddRange(finished.Where(season => season.StartsAt.Year == year).Select(RankingPeriod.ForSeason));

            periods.Add(RankingPeriod.ForYear(year));
        }

        return periods;
    }

    public async Task<HistoryModel?> FindAsync(string? historyId, CancellationToken cancellationToken = default)
    {
        var periods = await ListAsync(cancellationToken);
        if (periods.Count == 0)
            return null;

        var period = string.IsNullOrEmpty(historyId)
            ? periods[^1]
            : periods.FirstOrDefault(p => p.Id.Equals(historyId, StringComparison.OrdinalIgnoreCase)) ?? await PastSeasonAsync(historyId, cancellationToken);

        if (period == null)
            return null;

        return new HistoryModel { History = period, Players = await PlayersAsync(period, cancellationToken), AllHistory = periods };
    }

    public async Task<HistoryModel?> LastAsync(CancellationToken cancellationToken = default)
    {
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var seasons = await seasonService.ListAsync(cancellationToken);

        foreach (var season in seasons.Where(season => season.EndsAt <= now).OrderByDescending(season => season.StartsAt))
        {
            var players = await rankingService.SeasonAsync(season.Id, cancellationToken);

            if (players.Count > 0)
                return new HistoryModel { History = RankingPeriod.ForSeason(season), Players = players };
        }

        return null;
    }

    public async Task<string?> SeasonHistoryIdAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        var season = await seasonService.FindAsync(seasonId, cancellationToken);

        return season == null ? null : RankingPeriod.ForSeason(season).Id;
    }

    private async Task<RankingPeriod?> PastSeasonAsync(string historyId, CancellationToken cancellationToken)
    {
        var now = await seasonService.SiteNowAsync(cancellationToken);
        var seasons = await seasonService.ListAsync(cancellationToken);

        return seasons
            .Where(season => season.EndsAt <= now)
            .Select(RankingPeriod.ForSeason)
            .FirstOrDefault(period => period.Id.Equals(historyId, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<IReadOnlyList<RankingPlayer>> PlayersAsync(RankingPeriod period, CancellationToken cancellationToken)
    {
        return period.Kind switch
        {
            RankingPeriodKind.Season => await rankingService.SeasonAsync(period.Season!.Id, cancellationToken),
            RankingPeriodKind.Year => await rankingService.YearAsync(period.StartYear, cancellationToken),
            _ => await rankingService.AllTimeAsync(cancellationToken)
        };
    }
}
