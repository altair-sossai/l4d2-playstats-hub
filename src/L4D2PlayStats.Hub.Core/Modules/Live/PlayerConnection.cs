namespace L4D2PlayStats.Hub.Core.Modules.Live;

public class PlayerConnection
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = null!;
    public long CommunityId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public long Count { get; set; }
}