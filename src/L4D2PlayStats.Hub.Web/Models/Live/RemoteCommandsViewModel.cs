using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class RemoteCommandsViewModel
{
    public IReadOnlyList<GameServer> Servers { get; init; } = [];
    public IReadOnlyList<RemoteCommandLog> Logs { get; init; } = [];
}
