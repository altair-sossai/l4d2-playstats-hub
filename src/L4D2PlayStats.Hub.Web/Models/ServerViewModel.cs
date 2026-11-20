namespace L4D2PlayStats.Hub.Web.Models;

public class ServerViewModel
{
    public ServerStatusViewModel Status { get; init; } = null!;
    public string? Token { get; init; }
    public string SiteUrl { get; init; } = null!;
    public bool Wizard { get; init; }
}