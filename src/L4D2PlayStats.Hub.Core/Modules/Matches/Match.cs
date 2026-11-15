using L4D2PlayStats.Hub.Core.Modules.Seasons;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;

namespace L4D2PlayStats.Hub.Core.Modules.Matches;

public class Match
{
    public int Id { get; set; }
    public int GameServerId { get; set; }
    public GameServer? GameServer { get; set; }
    public int SeasonId { get; set; }
    public Season? Season { get; set; }
    public string? Campaign { get; set; }
    public string ConfigurationName { get; set; } = null!;
    public int TeamSize { get; set; }
    public DateTime FirstFileDate { get; set; }
    public DateTime LastFileDate { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public int TeamAScore { get; set; }
    public int TeamBScore { get; set; }
    public int MapCount { get; set; }
    public bool CountsForRanking { get; set; }
    public List<MatchPlayer> Players { get; set; } = [];
    public List<StatisticsFile> Files { get; set; } = [];
}
