using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
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

    public async Task<string?> PlayersPeriodAsync(string? historyId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(historyId))
            return null;

        if (historyId.Equals(AppConsts.AllTimeHistoryId, StringComparison.OrdinalIgnoreCase))
            return PlayerUrls.AllTimeQuery();

        if (int.TryParse(historyId, NumberStyles.None, CultureInfo.InvariantCulture, out var year))
            return PlayerUrls.YearQuery(year);

        var seasons = await seasonService.ListAsync(cancellationToken);
        var season = seasons.FirstOrDefault(season => RankingPeriod.ForSeason(season).Id.Equals(historyId, StringComparison.OrdinalIgnoreCase));

        return season == null ? null : PlayerUrls.SeasonQuery(season.Id);
    }
}
