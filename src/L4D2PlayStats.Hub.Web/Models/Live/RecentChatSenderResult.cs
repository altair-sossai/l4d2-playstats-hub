using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Web.Models.Players;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class RecentChatSenderResult(RecentChatSender sender) : SteamIdentityResult(sender.CommunityId)
{
    public string Name { get; } = sender.Name;
    public string Message { get; } = sender.Text;
    public DateTime When { get; } = sender.When;
    public bool AlreadyBanned { get; } = sender.Banned;
}
