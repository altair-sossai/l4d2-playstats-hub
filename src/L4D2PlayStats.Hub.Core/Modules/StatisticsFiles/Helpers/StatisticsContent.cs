using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;

public static class StatisticsContent
{
    public static bool IsComplete(ParsedStatistics statistics)
    {
        var gameRound = statistics.GameRound;

        if (gameRound == null || gameRound.Round <= 0 || gameRound.TeamSize <= 0)
            return false;

        if (string.IsNullOrEmpty(gameRound.ConfigurationName) || string.IsNullOrEmpty(gameRound.MapName))
            return false;

        if (statistics.Halves.Count != 2)
            return false;

        if (statistics.Halves.Any(half => half.RoundHalf == null || half.Progress == null || half.Players.Count == 0 || half.InfectedPlayers.Count == 0))
            return false;

        if (statistics.Scoring?.TeamA == null || statistics.Scoring.TeamB == null)
            return false;

        if (statistics.PlayerNames.Count == 0 || statistics.TeamA.Count == 0 || statistics.TeamB.Count == 0)
            return false;

        return statistics.MapStart != null && statistics.MapEnd != null;
    }
}
