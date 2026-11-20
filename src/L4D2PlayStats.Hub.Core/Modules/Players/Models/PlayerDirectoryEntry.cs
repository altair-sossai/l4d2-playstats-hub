namespace L4D2PlayStats.Hub.Core.Modules.Players.Models;

public class PlayerDirectoryEntry(long communityId, bool known = true)
{
    public long CommunityId { get; } = communityId;
    public bool Known { get; } = known;
    public string? Name { get; set; }
    public List<string> Names { get; } = [];
    public DateTime? LastSeenAtUtc { get; set; }
    public int? Position { get; set; }
    public bool Ranked { get; set; }
}