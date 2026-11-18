using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatAccessResult(AntiCheatAccessSnapshot access)
{
    public string Mode { get; } = access.Mode.ToName();
    public bool ModeSaved { get; } = access.ModeSaved;
    public string? UpdatedByName { get; } = access.UpdatedByName;
    public string? UpdatedFrom { get; } = access.UpdatedFrom?.ToString();
    public DateTime? UpdatedAtUtc { get; } = access.UpdatedAtUtc;
    public List<AntiCheatAccessPlayerResult> Whitelist { get; } = [.. access.Whitelist.Select(player => new AntiCheatAccessPlayerResult(player))];
    public List<AntiCheatAccessPlayerResult> Required { get; } = [.. access.Required.Select(player => new AntiCheatAccessPlayerResult(player))];
}
