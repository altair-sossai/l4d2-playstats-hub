namespace L4D2PlayStats.Hub.Core.Modules.GameModes;

public class GameConfiguration
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int TeamSize { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
}