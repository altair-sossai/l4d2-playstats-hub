using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class ChatBansViewModel
{
    public IReadOnlyList<ChatBan> Bans { get; init; } = [];
    public IReadOnlyList<RecentChatSender> RecentSenders { get; init; } = [];
}
