using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Web.Models.Live;

namespace L4D2PlayStats.Hub.Web.Services.Contracts;

public interface ILiveViewService
{
    Task<GameServer?> FindServerAsync(string publicId, CancellationToken cancellationToken = default);
    Task<List<GameServer>> EnabledServersAsync(CancellationToken cancellationToken = default);
    Task<ServerListViewModel> ListAsync(CancellationToken cancellationToken = default);
    Task<ServerCardViewModel?> MainServerAsync(CancellationToken cancellationToken = default);
    Task<ServerCardViewModel> CardAsync(GameServer server, CancellationToken cancellationToken = default);
    Task<LiveServerViewModel> LiveAsync(GameServer server, CancellationToken cancellationToken = default);
    Task<LiveFeedViewModel> FeedAsync(GameServer server, long after, CancellationToken cancellationToken = default);
    Task<TimeZoneInfo> TimeZoneAsync(CancellationToken cancellationToken = default);
}