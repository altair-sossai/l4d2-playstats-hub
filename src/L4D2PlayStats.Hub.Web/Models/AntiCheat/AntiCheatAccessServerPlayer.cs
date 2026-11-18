using L4D2PlayStats.Hub.Core.Modules.AntiCheat;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatAccessServerPlayer(AntiCheatAccessPlayer player)
{
    public long AccountId { get; } = player.CommunityId - CoreConsts.SteamCommunityIdBase;
    public string CommunityId { get; } = player.CommunityId.ToString();
    public string Name { get; } = string.IsNullOrWhiteSpace(player.Name) ? player.CommunityId.ToString() : player.Name;
}
