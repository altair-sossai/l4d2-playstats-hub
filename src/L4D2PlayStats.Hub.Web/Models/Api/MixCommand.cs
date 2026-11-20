namespace L4D2PlayStats.Hub.Web.Models.Api;

public class MixCommand
{
    public string? Player1 { get; set; }
    public string? Player2 { get; set; }
    public string? Player3 { get; set; }
    public string? Player4 { get; set; }
    public string? Player5 { get; set; }
    public string? Player6 { get; set; }
    public string? Player7 { get; set; }
    public string? Player8 { get; set; }

    public IEnumerable<string?> All => [Player1, Player2, Player3, Player4, Player5, Player6, Player7, Player8];
}