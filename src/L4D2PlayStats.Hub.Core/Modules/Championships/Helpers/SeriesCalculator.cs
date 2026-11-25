using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class SeriesCalculator
{
    public static int Majority(int bestOf)
    {
        return Math.Max(1, bestOf) / 2 + 1;
    }

    public static SeriesProgress Evaluate(ChampionshipSeries series, bool knockout)
    {
        if (series.Bye)
            return new SeriesProgress(0, 0, 0, 0, true, series.TeamAId ?? series.TeamBId, 0);

        if (series.Walkover)
            return new SeriesProgress(0, 0, 0, 0, series.WinnerTeamId != null, series.WinnerTeamId, 0);

        var played = series.Games.Where(game => game.IsPlayed).OrderBy(game => game.Number).ToList();
        var winsA = 0;
        var winsB = 0;
        var ties = 0;
        var majority = Majority(series.BestOf);
        var counted = 0;

        foreach (var game in played)
        {
            if (winsA >= majority || winsB >= majority)
                break;

            counted++;

            if (game.TeamATotal > game.TeamBTotal)
                winsA++;
            else if (game.TeamBTotal > game.TeamATotal)
                winsB++;
            else
                ties++;
        }

        var required = knockout ? series.BestOf + ties : series.BestOf;
        var decided = winsA >= majority || winsB >= majority;
        var finished = series.HasTeams && (decided || !knockout && counted >= series.BestOf);
        int? winner = !finished ? null : winsA > winsB ? series.TeamAId : winsB > winsA ? series.TeamBId : null;

        return new SeriesProgress(winsA, winsB, ties, counted, finished, winner, required);
    }
}
