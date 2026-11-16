using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class LiveServerViewModel
{
    public ServerCardViewModel Card { get; init; } = null!;
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public LiveFeedViewModel Feed { get; init; } = null!;
    public bool ChatEnabled { get; init; }
    public bool RemoteCommandsEnabled { get; init; }
    public ChatBan? ChatBan { get; init; }
    public bool Administrator { get; init; }
    public string SiteName { get; init; } = null!;
    public bool SingleServer => Servers.Count <= 1;
}
