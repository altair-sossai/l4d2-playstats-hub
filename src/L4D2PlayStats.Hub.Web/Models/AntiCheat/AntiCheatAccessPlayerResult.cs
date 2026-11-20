using L4D2PlayStats.Hub.Core.Modules.AntiCheat;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatAccessPlayerResult(AntiCheatAccessPlayer player)
{
    public string CommunityId { get; } = player.CommunityId.ToString();
    public long AccountId { get; } = player.CommunityId - CoreConsts.SteamCommunityIdBase;
    public string SteamId { get; } = SteamIds.SteamId(player.CommunityId);
    public string Steam3 { get; } = SteamIds.Steam3(player.CommunityId);
    public string ProfileUrl { get; } = SteamIds.ProfileUrl(player.CommunityId);
    public string Name { get; } = string.IsNullOrWhiteSpace(player.Name) ? player.CommunityId.ToString() : player.Name;
    public string? AddedByName { get; } = player.AddedByName;
    public string AddedFrom { get; } = player.AddedFrom.ToString();
    public DateTime CreatedAtUtc { get; } = player.CreatedAtUtc;
    public string AvatarUrl { get; } = $"{AppConsts.AvatarUrl}{player.CommunityId}";
}
