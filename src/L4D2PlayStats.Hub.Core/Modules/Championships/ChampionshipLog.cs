namespace L4D2PlayStats.Hub.Core.Modules.Championships;

public class ChampionshipLog
{
    public int Id { get; set; }
    public int ChampionshipId { get; set; }
    public long? CommunityId { get; set; }
    public string Text { get; set; } = null!;
    public string? Values { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
