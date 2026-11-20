using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Models;

public class BackupRun
{
    public DateTime StartedAtUtc { get; set; }
    public long DurationInMilliseconds { get; set; }
    public long SizeInBytes { get; set; }
    public string? BlobName { get; set; }
    public bool Succeeded { get; set; }
    public string? Error { get; set; }
    public BackupTrigger Trigger { get; set; }
    public BackupVerification? Verification { get; set; }
}
