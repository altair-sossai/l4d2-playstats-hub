using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipPodiums
{
    public static ChampionshipPodium Of(Championship championship)
    {
        var last = championship.Stages.OrderBy(stage => stage.Order).LastOrDefault();

        if (last == null || !last.Generated || last.Series.Count == 0)
            return ChampionshipPodium.None;

        if (last.IsKnockout)
        {
            var final = last.Series.Where(series => !series.ThirdPlace).MaxBy(series => series.Round);
            var third = last.Series.FirstOrDefault(series => series.ThirdPlace);

            if (final is not { Finished: true, WinnerTeamId: not null })
                return ChampionshipPodium.None;

            return new ChampionshipPodium(final.WinnerTeamId, final.LoserTeamId, third is { Finished: true } ? third.WinnerTeamId : null);
        }

        if (last.Groups.Count != 1 || last.Series.Any(series => !series.Finished))
            return ChampionshipPodium.None;

        var seeds = last.Groups[0].Teams.ToDictionary(team => team.TeamId, team => team.Seed);
        var standings = StandingsCalculator.Calculate(last, seeds, last.Series, last.Penalties);

        return new ChampionshipPodium(standings.ElementAtOrDefault(0)?.TeamId, standings.ElementAtOrDefault(1)?.TeamId, standings.ElementAtOrDefault(2)?.TeamId);
    }
}
