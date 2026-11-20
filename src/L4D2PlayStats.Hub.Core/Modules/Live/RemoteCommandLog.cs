namespace L4D2PlayStats.Hub.Core.Modules.Live;

public class RemoteCommandLog
{
    public int Id { get; set; }
    public int GameServerId { get; set; }
    public string GameServerName { get; set; } = null!;
    public long AdministratorCommunityId { get; set; }
    public string? AdministratorName { get; set; }
    public string Command { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
}