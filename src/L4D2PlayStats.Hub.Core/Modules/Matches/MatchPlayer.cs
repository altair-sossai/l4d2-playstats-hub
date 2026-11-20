namespace L4D2PlayStats.Hub.Core.Modules.Matches;

public class MatchPlayer : MatchPlayerStatistics
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match? Match { get; set; }
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public int Order { get; set; }
    public bool TeamA { get; set; }
    public bool TeamB { get; set; }
    public bool Win { get; set; }
    public bool Loss { get; set; }
    public int Draws { get; set; }
    public int RageQuits { get; set; }
    public bool Participant { get; set; }

    public int Teams => (TeamA ? 1 : 0) + (TeamB ? 1 : 0);
}