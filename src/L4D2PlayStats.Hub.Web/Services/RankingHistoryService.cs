using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class RankingHistoryService(ISeasonService seasonService, IRankingService rankingService) : IRankingHistoryService
{
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

    public async Task<string?> PlayersPeriodAsync(string historyId, CancellationToken cancellationToken = default)
    {
        if (historyId.Equals(AppConsts.AllTimeHistoryId, StringComparison.OrdinalIgnoreCase))
            return PlayerUrls.AllTimeQuery();

        if (int.TryParse(historyId, NumberStyles.None, CultureInfo.InvariantCulture, out var year))
            return await YearPeriodAsync(year, cancellationToken);

        var seasons = await StartedAsync(cancellationToken);
        var season = seasons.FirstOrDefault(season => RankingPeriod.ForSeason(season).Id.Equals(historyId, StringComparison.OrdinalIgnoreCase));

        return season == null ? null : PlayerUrls.SeasonQuery(season.Id);
    }

    public async Task<string?> SeasonPeriodAsync(int seasonId, CancellationToken cancellationToken = default)
    {
        var seasons = await StartedAsync(cancellationToken);

        return seasons.Any(season => season.Id == seasonId) ? PlayerUrls.SeasonQuery(seasonId) : null;
    }

    public async Task<string?> YearPeriodAsync(int year, CancellationToken cancellationToken = default)
    {
        var seasons = await StartedAsync(cancellationToken);

        return seasons.Any(season => season.StartsAt.Year == year) ? PlayerUrls.YearQuery(year) : null;
    }

    private async Task<List<Season>> StartedAsync(CancellationToken cancellationToken)
    {
        var current = await seasonService.GetCurrentAsync(cancellationToken);
        var seasons = await seasonService.ListAsync(cancellationToken);

        return [.. seasons.Where(season => season.StartsAt <= current.StartsAt)];
    }
}
