namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipDraftPick
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public int Number { get; set; }
    public int TeamId { get; set; }
    public long CommunityId { get; set; }
    public DateTime PickedAtUtc { get; set; }
}
