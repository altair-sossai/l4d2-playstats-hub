namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public class GameUnbanCommand
{
    public string? Player { get; set; }
    public string? Command { get; set; }
    public string? AdminCommunityId { get; set; }
    public string? AdminName { get; set; }
}
