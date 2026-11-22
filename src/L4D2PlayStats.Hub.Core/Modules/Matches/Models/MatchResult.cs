namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public class MatchResult
{
    public int Id { get; init; }
    public int GameServerId { get; init; }
    public string? GameServerName { get; init; }
    public int SeasonId { get; init; }
    public string? ConfigurationName { get; init; }
    public bool Competitive { get; init; }
    public int TeamSize { get; init; }
    public DateTime FirstFileDate { get; init; }
    public DateTime LastFileDate { get; init; }
    public DateTime MatchStart { get; init; }
    public bool MatchStartIsUtc { get; init; } = true;
    public DateTime? MatchEnd { get; init; }
    public TimeSpan? MatchElapsed => MatchStartIsUtc ? MatchEnd - MatchStart : null;
    public string? Campaign { get; init; }
    public IReadOnlyList<MatchTeamResult> Teams { get; init; } = [];
    public IReadOnlyList<MatchMapResult> Maps { get; init; } = [];
    public IReadOnlyDictionary<long, string> PlayerNames { get; init; } = new Dictionary<long, string>();

    public MatchTeamResult? Winner => Teams.MaxBy(team => team.Score);
    public MatchTeamResult? TeamA => Teams.FirstOrDefault();
    public MatchTeamResult? TeamB => Teams.LastOrDefault();

    public int ScoreA => TeamA?.Score ?? 0;
    public int ScoreB => TeamB?.Score ?? 0;
    public int Diff => Math.Abs(ScoreA - ScoreB);

    public bool Draw => ScoreA == ScoreB;
    public bool TeamAWon => ScoreA > ScoreB;
    public bool TeamBWon => ScoreB > ScoreA;
}
