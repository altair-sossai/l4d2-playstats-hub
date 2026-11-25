namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipTeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public long CommunityId { get; set; }
    public DateTime JoinedAtUtc { get; set; }
}
