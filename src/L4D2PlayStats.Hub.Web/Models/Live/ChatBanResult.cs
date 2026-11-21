using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class ChatBanResult(ChatBan ban) : SteamIdentityResult(ban.CommunityId)
{
    public string Name { get; } = ban.Name ?? string.Empty;
    public string? Reason { get; } = ban.Reason;
    public DateTime CreatedAtUtc { get; } = ban.CreatedAtUtc;
    public DateTime? UpdatedAtUtc { get; } = ban.UpdatedAtUtc;
    public string? CreatedByName { get; } = ban.CreatedByName;
    public string? CreatedByCommunityId { get; } = ban.CreatedByCommunityId?.ToString();
}
