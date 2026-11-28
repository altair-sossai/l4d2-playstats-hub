namespace L4D2PlayStats.Hub.Core.Modules.Bans.Models;

public class GameBanCommand
{
    public string? Player { get; set; }
    public string? Name { get; set; }
    public int Minutes { get; set; }
    public string? Reason { get; set; }
    public string? Command { get; set; }
    public string? AdminCommunityId { get; set; }
    public string? AdminName { get; set; }
}
