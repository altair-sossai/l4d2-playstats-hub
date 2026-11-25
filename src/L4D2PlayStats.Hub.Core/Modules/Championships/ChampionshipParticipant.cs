namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipParticipant
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public long CommunityId { get; set; }
    public int Pot { get; set; }
    public DateTime AddedAtUtc { get; set; }
}
