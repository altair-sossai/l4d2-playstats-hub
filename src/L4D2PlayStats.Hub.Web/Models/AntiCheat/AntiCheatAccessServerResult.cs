using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatAccessServerResult(AntiCheatAccessSnapshot access)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string? Mode { get; } = access.ModeSaved ? access.Mode.ToName() : null;

    public List<AntiCheatAccessServerPlayer> Whitelist { get; } = [.. access.Whitelist.Select(player => new AntiCheatAccessServerPlayer(player))];
    public List<AntiCheatAccessServerPlayer> Required { get; } = [.. access.Required.Select(player => new AntiCheatAccessServerPlayer(player))];
}