using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Helpers;

public static class CommunityRatingFit
{
    public static double Expected(double rating, double opponent)
    {
        return 1 / (1 + Math.Pow(AppConsts.CommunityEloBase, (opponent - rating) / AppConsts.CommunityEloScale));
    }

    public static double Score(CommunityVoteOutcome outcome)
    {
        return outcome switch
        {
            CommunityVoteOutcome.Left => AppConsts.CommunityWinScore,
            CommunityVoteOutcome.Right => AppConsts.CommunityLossScore,
            _ => AppConsts.CommunityDrawScore
        };
    }

    public static Dictionary<long, CommunityStrength> Fit(IReadOnlyCollection<CommunityComparison> comparisons, double center)
    {
        var players = comparisons.SelectMany(comparison => new[] { comparison.LeftPlayerId, comparison.RightPlayerId }).Distinct().Order().ToList();
        var index = players.Select((player, position) => (player, position)).ToDictionary(entry => entry.player, entry => entry.position);
        var games = players.Select(_ => new List<(int Opponent, double Score)>()).ToList();

        foreach (var comparison in comparisons)
        {
            var left = index[comparison.LeftPlayerId];
            var right = index[comparison.RightPlayerId];

            games[left].Add((right, comparison.LeftScore));
            games[right].Add((left, 1 - comparison.LeftScore));
        }

        var precision = Math.Pow(AppConsts.CommunityPointsPerStrength / AppConsts.CommunityPriorDeviation, 2);
        var strength = new double[players.Count];

        for (var iteration = 0; iteration < AppConsts.CommunityFitIterations; iteration++)
        {
            var largest = 0d;

            for (var player = 0; player < players.Count; player++)
            {
                var (gradient, information) = Slope(player, games[player], strength, precision);
                var step = Math.Clamp(gradient / information, -AppConsts.CommunityFitMaxStep, AppConsts.CommunityFitMaxStep);

                strength[player] += step;
                largest = Math.Max(largest, Math.Abs(step));
            }

            if (largest < AppConsts.CommunityFitTolerance)
                break;
        }

        return players
            .Select((player, position) => (player, position))
            .ToDictionary(
                entry => entry.player,
                entry => new CommunityStrength(
                    center + strength[entry.position] * AppConsts.CommunityPointsPerStrength,
                    AppConsts.CommunityPointsPerStrength / Math.Sqrt(Slope(entry.position, games[entry.position], strength, precision).Information)));
    }

    private static (double Gradient, double Information) Slope(int player, List<(int Opponent, double Score)> games, double[] strength, double precision)
    {
        var gradient = -strength[player] * precision;
        var information = precision;

        foreach (var (opponent, score) in games)
        {
            var expected = 1 / (1 + Math.Exp(strength[opponent] - strength[player]));

            gradient += score - expected;
            information += expected * (1 - expected);
        }

        return (gradient, information);
    }
}
