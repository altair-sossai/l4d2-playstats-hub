using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat;

public class AntiCheatAccessSetting
{
    public int Id { get; set; }
    public AntiCheatAccessMode Mode { get; set; }
    public long? UpdatedByCommunityId { get; set; }
    public string? UpdatedByName { get; set; }
    public AntiCheatAccessSource UpdatedFrom { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
