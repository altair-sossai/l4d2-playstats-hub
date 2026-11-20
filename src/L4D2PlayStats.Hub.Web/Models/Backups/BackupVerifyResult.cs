using L4D2PlayStats.Hub.Core.Modules.Backups.Models;

namespace L4D2PlayStats.Hub.Web.Models.Backups;

public record BackupVerifyResult(string? BlobName, BackupVerification Verification);
