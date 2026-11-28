using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class StandingsCalculator
{
    public static List<ChampionshipStanding> Calculate(ChampionshipStage stage, IReadOnlyDictionary<int, int> teamSeeds, IEnumerable<ChampionshipSeries> series, IEnumerable<ChampionshipPenalty> penalties)
    {
        var rows = teamSeeds.ToDictionary(pair => pair.Key, pair => new ChampionshipStanding { TeamId = pair.Key, Seed = pair.Value });
        var counted = series.Where(item => item.HasTeams && !item.Bye && rows.ContainsKey(item.TeamAId!.Value) && rows.ContainsKey(item.TeamBId!.Value)).ToList();

        foreach (var item in counted)
            Add(stage, rows[item.TeamAId!.Value], rows[item.TeamBId!.Value], item);

        foreach (var penalty in penalties)
            if (rows.TryGetValue(penalty.TeamId, out var row))
                row.PenaltyPoints += penalty.Points;

        var tiebreakers = ChampionshipTiebreakers.Parse(stage.Tiebreakers);
        var ordered = Order(rows.Values.ToList(), tiebreakers, 0, stage, counted, true);

        for (var index = 0; index < ordered.Count; index++)
            ordered[index].Position = index + 1;

        return ordered;
    }

    private static void Add(ChampionshipStage stage, ChampionshipStanding a, ChampionshipStanding b, ChampionshipSeries series)
    {
        var progress = SeriesCalculator.Evaluate(series, false);

        foreach (var game in series.Games.Where(game => game.IsPlayed))
        {
            a.ScoreFor += game.TeamAScore;
            a.ScoreAgainst += game.TeamBScore;
            a.Penalties += game.TeamAPenalty;
            b.ScoreFor += game.TeamBScore;
            b.ScoreAgainst += game.TeamAScore;
            b.Penalties += game.TeamBPenalty;
        }

        a.GameWins += progress.WinsA;
        a.GameLosses += progress.WinsB;
        b.GameWins += progress.WinsB;
        b.GameLosses += progress.WinsA;

        if (!progress.Finished)
            return;

        a.Played++;
        b.Played++;

        if (progress.WinnerTeamId == null)
        {
            a.Draws++;
            b.Draws++;
            a.MatchPoints += stage.PointsForDraw;
            b.MatchPoints += stage.PointsForDraw;
        }
        else if (progress.WinnerTeamId == a.TeamId)
        {
            Result(stage, a, b);
        }
        else
        {
            Result(stage, b, a);
        }
    }

    private static void Result(ChampionshipStage stage, ChampionshipStanding winner, ChampionshipStanding loser)
    {
        winner.Wins++;
        loser.Losses++;
        winner.MatchPoints += stage.PointsForWin;
        loser.MatchPoints += stage.PointsForLoss;
    }

    private static List<ChampionshipStanding> Order(List<ChampionshipStanding> rows, IReadOnlyList<ChampionshipTiebreaker> tiebreakers, int index, ChampionshipStage stage, IReadOnlyList<ChampionshipSeries> series, bool byPoints)
    {
        if (rows.Count <= 1)
            return rows;

        if (byPoints)
            return rows
                .GroupBy(row => row.Points)
                .OrderByDescending(group => group.Key)
                .SelectMany(group => Order(group.ToList(), tiebreakers, 0, stage, series, false))
                .ToList();

        if (index >= tiebreakers.Count)
            return rows.OrderBy(row => row.Seed).ThenBy(row => row.TeamId).ToList();

        var keys = Keys(rows, tiebreakers[index], stage, series);

        if (keys.Values.Distinct().Count() == 1)
            return Order(rows, tiebreakers, index + 1, stage, series, false);

        return rows
            .GroupBy(row => keys[row.TeamId])
            .OrderByDescending(group => group.Key)
            .SelectMany(group => Order(group.ToList(), tiebreakers, index + 1, stage, series, false))
            .ToList();
    }

    private static Dictionary<int, int> Keys(List<ChampionshipStanding> rows, ChampionshipTiebreaker tiebreaker, ChampionshipStage stage, IReadOnlyList<ChampionshipSeries> series)
    {
        return tiebreaker switch
        {
            ChampionshipTiebreaker.Difference => rows.ToDictionary(row => row.TeamId, row => row.Difference),
            ChampionshipTiebreaker.ScoreFor => rows.ToDictionary(row => row.TeamId, row => row.ScoreFor),
            ChampionshipTiebreaker.Wins => rows.ToDictionary(row => row.TeamId, row => row.Wins),
            _ => HeadToHead(rows, stage, series)
        };
    }

    private static Dictionary<int, int> HeadToHead(List<ChampionshipStanding> rows, ChampionshipStage stage, IReadOnlyList<ChampionshipSeries> series)
    {
        var teams = rows.Select(row => row.TeamId).ToHashSet();
        var mini = rows.ToDictionary(row => row.TeamId, row => new ChampionshipStanding { TeamId = row.TeamId, Seed = row.Seed });

        foreach (var item in series.Where(item => teams.Contains(item.TeamAId!.Value) && teams.Contains(item.TeamBId!.Value)))
            Add(stage, mini[item.TeamAId!.Value], mini[item.TeamBId!.Value], item);

        return mini.ToDictionary(pair => pair.Key, pair => pair.Value.MatchPoints);
    }
}
