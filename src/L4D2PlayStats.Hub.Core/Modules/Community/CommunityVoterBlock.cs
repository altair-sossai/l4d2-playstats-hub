namespace L4D2PlayStats.Hub.Core.Modules.Community;

public class CommunityVoterBlock
{
    public long CommunityId { get; set; }
    public DateTime BlockedAtUtc { get; set; }
    public long? BlockedBy { get; set; }
}
