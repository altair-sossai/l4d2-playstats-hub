namespace L4D2PlayStats.Hub.Core.Modules.Punishments;

public class Punishment
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public long CommunityId { get; set; }
    public int Points { get; set; }
    public string? Reason { get; set; }
    public long? CreatedByCommunityId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
