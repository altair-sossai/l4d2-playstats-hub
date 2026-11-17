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
    public required string CampaignImage { get; init; }
    public bool RankingEnabled { get; init; }
    public bool AntiCheatPage { get; init; }
    public bool Online => Status.State == GameServerConnectionState.Online;
    public string? Map => Status.Signal?.Map;
    public string Name => string.IsNullOrWhiteSpace(Status.Signal?.Hostname) ? Server.Name : Status.Signal.Hostname;
    public string? ServerIp => Server.Address;
    public string? ServerDns => Server.AlternativeAddress;
    public int Players => Snapshot.PlayerCount;
    public int MaxPlayers => Status.Signal?.MaxPlayers ?? (Snapshot.Configuration is { TeamSize: > 0 } configuration ? configuration.TeamSize : AppConsts.DefaultTeamSize) * 2;
    public bool AnyPlayerConnected => Snapshot.AnyPlayerConnected;
    public string? ConnectUrl => string.IsNullOrEmpty(Server.Address) ? null : $"{AppConsts.SteamConnectUrl}{Server.Address}";
}
