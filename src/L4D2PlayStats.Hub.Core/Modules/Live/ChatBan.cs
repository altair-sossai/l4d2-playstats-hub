namespace L4D2PlayStats.Hub.Core.Modules.Live;

public class ChatBan
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public string? Reason { get; set; }
    public long? CreatedByCommunityId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
