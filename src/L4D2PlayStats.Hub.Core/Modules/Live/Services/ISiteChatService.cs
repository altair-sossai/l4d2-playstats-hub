using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface ISiteChatService
{
    Task<SiteChatResult> SendAsync(GameServer server, SiteChatUser user, string? message, CancellationToken cancellationToken = default);
    Task<List<RecentChatSender>> RecentSendersAsync(CancellationToken cancellationToken = default);
}
