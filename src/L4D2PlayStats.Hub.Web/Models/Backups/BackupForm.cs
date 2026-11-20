using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;

namespace L4D2PlayStats.Hub.Web.Models.Backups;

public class BackupForm
{
    public bool Enabled { get; set; }
    public BackupDestination Destination { get; set; }
    public string? ContainerUrl { get; set; }
    public string? ConnectionString { get; set; }
    public string? ContainerName { get; set; }
    public int IntervalInHours { get; set; }
    public int Keep { get; set; }
    public string? Prefix { get; set; }
}