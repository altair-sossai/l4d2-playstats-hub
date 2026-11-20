using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class ChatBanResult(ChatBan ban)
{
    public string CommunityId { get; } = ban.CommunityId.ToString();
    public string SteamId { get; } = SteamIds.SteamId(ban.CommunityId);
    public string Steam3 { get; } = SteamIds.Steam3(ban.CommunityId);
    public string ProfileUrl { get; } = SteamIds.ProfileUrl(ban.CommunityId);
    public string Name { get; } = ban.Name ?? string.Empty;
    public string? Reason { get; } = ban.Reason;
    public DateTime CreatedAtUtc { get; } = ban.CreatedAtUtc;
    public DateTime? UpdatedAtUtc { get; } = ban.UpdatedAtUtc;
    public string? CreatedByName { get; } = ban.CreatedByName;
    public string AvatarUrl { get; } = $"{AppConsts.AvatarUrl}{ban.CommunityId}";
}