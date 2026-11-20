using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat;

public abstract class AntiCheatAccessPlayer
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public long? AddedByCommunityId { get; set; }
    public string? AddedByName { get; set; }
    public AntiCheatAccessSource AddedFrom { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
