using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

public static class CommunityMixBalancer
{
    public static CommunityMixSplit Split(IReadOnlyList<CommunityMixPlayer> players, ICommunityRandomService random)
    {
        var options = Options(players);
        var best = options.Min(option => option.Cost);
        var close = options.Where(option => option.Cost <= best + AppConsts.CommunityMixCostTolerance).OrderBy(option => option.Key, StringComparer.Ordinal).ToList();
        var chosen = close[random.Next(close.Count)];
        var chance = Chance(chosen.TeamA, chosen.TeamB);

        return random.Next(AppConsts.CommunityPairSides) == 0
            ? new CommunityMixSplit(Ids(chosen.TeamA), Ids(chosen.TeamB), chance)
            : new CommunityMixSplit(Ids(chosen.TeamB), Ids(chosen.TeamA), 1 - chance);
    }

    public static List<CommunityMixOption> Options(IReadOnlyList<CommunityMixPlayer> players)
    {
        var ordered = players.OrderBy(player => player.CommunityId).ToList();
        var options = new List<CommunityMixOption>();

        foreach (var team in Teams(ordered.Count, ordered.Count / 2))
        {
            var teamA = team.Select(index => ordered[index]).ToList();
            var teamB = ordered.Where((_, index) => !team.Contains(index)).ToList();

            options.Add(new CommunityMixOption(teamA, teamB, Cost(teamA, teamB), string.Join(",", teamA.Select(player => player.CommunityId))));
        }

        return options;
    }

    public static double Cost(IReadOnlyList<CommunityMixPlayer> teamA, IReadOnlyList<CommunityMixPlayer> teamB)
    {
        return Math.Abs(Mean(teamA) - Mean(teamB))
               + AppConsts.CommunityMixStarWeight * Math.Abs(teamA.Max(player => player.Rating) - teamB.Max(player => player.Rating))
               + AppConsts.CommunityMixSpreadWeight * Math.Abs(Spread(teamA) - Spread(teamB))
               + AppConsts.CommunityMixUncertaintyWeight * Math.Abs(Uncertainty(teamA) - Uncertainty(teamB));
    }

    public static double Chance(IReadOnlyList<CommunityMixPlayer> team, IReadOnlyList<CommunityMixPlayer> opponents)
    {
        return CommunityRatingFit.Expected(Mean(team), Mean(opponents));
    }

    private static IEnumerable<List<int>> Teams(int count, int size)
    {
        return Pick([0], 1, count, size);
    }

    private static IEnumerable<List<int>> Pick(List<int> chosen, int next, int count, int size)
    {
        if (chosen.Count == size)
        {
            yield return chosen;
            yield break;
        }

        for (var index = next; index <= count - (size - chosen.Count); index++)
            foreach (var team in Pick([.. chosen, index], index + 1, count, size))
                yield return team;
    }

    private static double Mean(IReadOnlyList<CommunityMixPlayer> team)
    {
        return team.Average(player => player.Rating);
    }

    private static double Spread(IReadOnlyList<CommunityMixPlayer> team)
    {
        var mean = Mean(team);

        return Math.Sqrt(team.Average(player => Math.Pow(player.Rating - mean, 2)));
    }

    private static double Uncertainty(IReadOnlyList<CommunityMixPlayer> team)
    {
        return Math.Sqrt(team.Average(player => player.Deviation * player.Deviation));
    }

    private static List<long> Ids(IReadOnlyList<CommunityMixPlayer> team)
    {
        return [.. team.Select(player => player.CommunityId)];
    }
}
