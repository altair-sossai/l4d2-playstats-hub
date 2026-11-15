namespace L4D2PlayStats.Hub.Core.Modules.Servers;

public class GameServer
{
    public int Id { get; set; }
    public string PublicId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string KeyHash { get; set; } = null!;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
}

public record GameServerCredentials(GameServer Server, string Key)
{
    public string Token => $"{Server.PublicId}{AppConsts.GameServerTokenSeparator}{Key}";
}

public enum GameServerAuthenticationStatus
{
    Success,
    MissingToken,
    UnknownServer,
    InvalidKey,
    Disabled
}

public record GameServerAuthentication(GameServerAuthenticationStatus Status, GameServer? Server)
{
    public bool Succeeded => Status == GameServerAuthenticationStatus.Success && Server != null;
}

public enum GameServerConnectionState
{
    Waiting,
    Online,
    Offline,
    InvalidKey
}

public class GameServerSignal
{
    public DateTime ReceivedAtUtc { get; set; }
    public string? Hostname { get; set; }
    public string? Map { get; set; }
    public string? Configuration { get; set; }
    public string? SourceModVersion { get; set; }
    public bool? StatisticsWriting { get; set; }
    public Dictionary<string, string> Plugins { get; set; } = [];
}

public class GameServerStatus
{
    public GameServerConnectionState State { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? InvalidKeyAtUtc { get; set; }
    public GameServerSignal? Signal { get; set; }
    public DateTime? LastStatisticsAtUtc { get; set; }
    public DateTime? LastLiveDataAtUtc { get; set; }
}
