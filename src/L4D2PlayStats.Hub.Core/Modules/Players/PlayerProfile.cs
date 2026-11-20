namespace L4D2PlayStats.Hub.Core.Modules.Players;

public class PlayerProfile
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? AvatarUpdatedAtUtc { get; set; }
}
