namespace L4D2PlayStats.Hub.Core.Modules.Skins;

public class SkinAuthor
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public DateTime SyncedAtUtc { get; set; }
    public DateTime? RenamedAtUtc { get; set; }
}
