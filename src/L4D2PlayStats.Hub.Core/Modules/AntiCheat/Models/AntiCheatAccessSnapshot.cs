using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;

public class AntiCheatAccessSnapshot
{
    public AntiCheatAccessMode Mode { get; init; } = AppConsts.AntiCheatDefaultMode;
    public bool ModeSaved { get; init; }
    public string? UpdatedByName { get; init; }
    public AntiCheatAccessSource? UpdatedFrom { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public IReadOnlyList<AntiCheatAccessPlayer> Whitelist { get; init; } = [];
    public IReadOnlyList<AntiCheatAccessPlayer> Required { get; init; } = [];

    public IReadOnlyList<AntiCheatAccessPlayer> this[AntiCheatAccessList list] => list == AntiCheatAccessList.Whitelist ? Whitelist : Required;

    public bool IsRequired(long communityId)
    {
        return Mode == AntiCheatAccessMode.Everyone
            ? Whitelist.All(player => player.CommunityId != communityId)
            : Required.Any(player => player.CommunityId == communityId);
    }
}
