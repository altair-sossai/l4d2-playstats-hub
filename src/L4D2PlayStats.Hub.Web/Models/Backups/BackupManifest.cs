namespace L4D2PlayStats.Hub.Web.Models.Backups;

public class BackupManifest
{
    public string HubVersion { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string Provider { get; set; } = string.Empty;
    public bool IncludesDatabase { get; set; }
    public List<string> Files { get; set; } = [];
}
