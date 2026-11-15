using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;

public class StatisticsFile
{
    public int Id { get; set; }
    public int GameServerId { get; set; }
    public GameServer? GameServer { get; set; }
    public int SeasonId { get; set; }
    public string FileName { get; set; } = null!;
    public DateTime FileDate { get; set; }
    public string Content { get; set; } = null!;
    public int Round { get; set; }
    public int TeamSize { get; set; }
    public string ConfigurationName { get; set; } = null!;
    public string MapName { get; set; } = null!;
    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public int? MatchId { get; set; }
    public Match? Match { get; set; }
    public int? MatchOrder { get; set; }

    public void Apply(ParsedStatistics statistics)
    {
        Round = statistics.GameRound?.Round ?? 0;
        TeamSize = statistics.GameRound?.TeamSize ?? 0;
        ConfigurationName = statistics.GameRound?.ConfigurationName ?? string.Empty;
        MapName = statistics.GameRound?.MapName ?? string.Empty;
        TeamAScore = statistics.Scoring?.TeamA?.Score ?? 0;
        TeamBScore = statistics.Scoring?.TeamB?.Score ?? 0;
        StartedAtUtc = statistics.MapStart;
        EndedAtUtc = statistics.MapEnd;
    }
}

public static partial class StatisticsFileName
{
    public static DateTime? ToDate(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return null;

        var match = Pattern().Match(fileName);
        if (!match.Success)
            return null;

        var year = int.Parse(match.Groups[1].Value);
        var month = int.Parse(match.Groups[2].Value);
        var day = int.Parse(match.Groups[3].Value);
        var hour = int.Parse(match.Groups[4].Value);
        var minute = int.Parse(match.Groups[5].Value);
        var sequence = int.Parse(match.Groups[6].Value);

        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month) || hour > 23 || minute > 59)
            return null;

        return new DateTime(year, month, day, hour, minute, 0).AddMilliseconds(sequence);
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"^(\d{4})-(\d{2})-(\d{2})_(\d{2})-(\d{2})_(\d{4})_.+\.txt$")]
    private static partial System.Text.RegularExpressions.Regex Pattern();
}

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
