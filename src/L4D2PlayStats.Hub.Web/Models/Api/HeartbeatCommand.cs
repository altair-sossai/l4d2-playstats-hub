namespace L4D2PlayStats.Hub.Web.Models.Api;

public class HeartbeatCommand
{
    public string? Hostname { get; set; }
    public string? Map { get; set; }
    public string? Configuration { get; set; }
    public int? MaxPlayers { get; set; }
    public string? SourceModVersion { get; set; }
    public bool? StatisticsWriting { get; set; }
    public Dictionary<string, string>? Plugins { get; set; }
}