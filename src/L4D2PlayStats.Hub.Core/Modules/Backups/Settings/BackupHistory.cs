using L4D2PlayStats.Hub.Core.Modules.Backups.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

public class BackupHistory
{
    public List<BackupRun> Runs { get; set; } = [];
}