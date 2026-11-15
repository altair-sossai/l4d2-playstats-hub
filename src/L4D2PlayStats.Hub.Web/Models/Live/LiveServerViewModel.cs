using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class LiveServerViewModel
{
    public ServerCardViewModel Card { get; init; } = null!;
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public LiveFeedViewModel Feed { get; init; } = null!;
    public SiteChatFormViewModel? Chat { get; init; }
}
