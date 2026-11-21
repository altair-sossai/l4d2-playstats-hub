namespace L4D2PlayStats.Hub.Core.Modules.Players;

public class ForcedName
{
    public long CommunityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
    public long? UpdatedBy { get; set; }
}
