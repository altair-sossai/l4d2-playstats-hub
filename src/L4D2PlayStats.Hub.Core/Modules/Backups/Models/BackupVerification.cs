using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Models;

public class BackupVerification
{
    public BackupVerificationStatus Status { get; set; }
    public DateTime VerifiedAtUtc { get; set; }
    public long DurationInMilliseconds { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Arguments { get; set; } = [];

    public static BackupVerification Of(BackupVerificationStatus status, string message, params string[] arguments)
    {
        return new BackupVerification { Status = status, Message = message, Arguments = [.. arguments] };
    }
}