using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class RecentChatSenderResult(RecentChatSender sender)
{
    public string CommunityId { get; } = sender.CommunityId.ToString();
    public string SteamId { get; } = SteamIds.SteamId(sender.CommunityId);
    public string Steam3 { get; } = SteamIds.Steam3(sender.CommunityId);
    public string ProfileUrl { get; } = SteamIds.ProfileUrl(sender.CommunityId);
    public string Name { get; } = sender.Name;
    public string Message { get; } = sender.Text;
    public DateTime When { get; } = sender.When;
    public bool AlreadyBanned { get; } = sender.Banned;
    public string AvatarUrl { get; } = $"{AppConsts.AvatarUrl}{sender.CommunityId}";
}
