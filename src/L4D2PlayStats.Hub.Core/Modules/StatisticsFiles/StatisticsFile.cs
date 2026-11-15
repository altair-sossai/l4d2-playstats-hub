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
