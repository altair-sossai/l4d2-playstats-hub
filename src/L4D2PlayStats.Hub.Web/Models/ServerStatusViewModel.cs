using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Web.Models;

public class ServerStatusViewModel
{
    public GameServer Server { get; init; } = null!;
    public GameServerStatus Status { get; init; } = null!;
    public ModuleSettings Modules { get; init; } = null!;
    public int Files { get; init; }
}