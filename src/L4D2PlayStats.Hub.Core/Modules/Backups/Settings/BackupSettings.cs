using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

public class BackupSettings
{
    public bool Enabled { get; set; }
    public BackupDestination Destination { get; set; } = BackupDestination.ContainerUrl;
    public string? ContainerUrl { get; set; }
    public string? ConnectionString { get; set; }
    public string? ContainerName { get; set; }
    public int IntervalInHours { get; set; } = AppConsts.BackupDefaultIntervalInHours;
    public int Keep { get; set; } = AppConsts.BackupDefaultKeep;
    public string Prefix { get; set; } = AppConsts.BackupDefaultPrefix;
}
