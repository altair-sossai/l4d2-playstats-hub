using L4D2PlayStats.Hub.Core.Modules.Servers.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Servers.Models;

public class GameServerStatus
{
    public GameServerConnectionState State { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? InvalidKeyAtUtc { get; set; }
    public GameServerSignal? Signal { get; set; }
    public DateTime? LastStatisticsAtUtc { get; set; }
    public DateTime? LastLiveDataAtUtc { get; set; }
}
