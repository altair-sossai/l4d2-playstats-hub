using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class ServerCardViewModel
{
    public GameServer Server { get; init; } = null!;
    public GameServerStatus Status { get; init; } = null!;
    public LiveSnapshot Snapshot { get; init; } = null!;
    public string? Campaign { get; init; }
    public bool RankingEnabled { get; init; }
    public bool Online => Status.State == GameServerConnectionState.Online;
    public string? Map => Status.Signal?.Map;
    public string? ConnectUrl => string.IsNullOrEmpty(Server.Address) ? null : $"{AppConsts.SteamConnectUrl}{Server.Address}";
}
