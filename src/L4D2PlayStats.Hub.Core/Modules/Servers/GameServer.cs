namespace L4D2PlayStats.Hub.Core.Modules.Servers;

public class GameServer
{
    public int Id { get; set; }
    public string PublicId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? AlternativeAddress { get; set; }
    public string KeyHash { get; set; } = null!;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
}
