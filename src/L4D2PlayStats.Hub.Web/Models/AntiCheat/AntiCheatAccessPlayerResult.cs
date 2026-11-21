using L4D2PlayStats.Hub.Core.Modules.AntiCheat;
using L4D2PlayStats.Hub.Web.Models.Players;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatAccessPlayerResult(AntiCheatAccessPlayer player) : SteamIdentityResult(player.CommunityId)
{
    public long AccountId { get; } = player.CommunityId - CoreConsts.SteamCommunityIdBase;
    public string Name { get; } = string.IsNullOrWhiteSpace(player.Name) ? player.CommunityId.ToString() : player.Name;
    public string? AddedByName { get; } = player.AddedByName;
    public string AddedFrom { get; } = player.AddedFrom.ToString();
    public DateTime CreatedAtUtc { get; } = player.CreatedAtUtc;
}
