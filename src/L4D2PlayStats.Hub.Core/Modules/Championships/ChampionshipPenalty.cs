namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipPenalty
{
    public int Id { get; set; }
    public int StageId { get; set; }
    public int TeamId { get; set; }
    public int Points { get; set; }
    public string? Reason { get; set; }
    public long? CreatedByCommunityId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
