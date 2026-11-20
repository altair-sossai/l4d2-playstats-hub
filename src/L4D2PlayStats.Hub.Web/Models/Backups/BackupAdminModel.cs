using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Models;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Web.Models.Backups;

public class BackupAdminModel(BackupSettings settings, BackupHistory history, bool includesDatabase, bool isRunning, TimeZoneInfo timeZone, SecretsKey secretsKey)
{
    public BackupSettings Settings { get; } = settings;
    public IReadOnlyList<BackupRun> Runs { get; } = history.Runs.OrderByDescending(run => run.StartedAtUtc).ToList();
    public bool IncludesDatabase { get; } = includesDatabase;
    public bool IsRunning { get; } = isRunning;
    public bool IsConfigured { get; } = BackupRules.IsConfigured(settings);
    public string? MaskedContainerUrl { get; } = BackupSecrets.MaskContainerUrl(settings.ContainerUrl);
    public string? MaskedConnectionString { get; } = BackupSecrets.MaskConnectionString(settings.ConnectionString);
    public BackupRun? LastSuccess { get; } = history.Runs.Where(run => run.Succeeded).MaxBy(run => run.StartedAtUtc);
    public DateTime? NextAtUtc { get; } = BackupSchedule.NextAtUtc(settings, history);
    public BackupVerification? LastVerification { get; } = history.Runs.Select(run => run.Verification).Where(verification => verification != null).MaxBy(verification => verification!.VerifiedAtUtc);
    public bool CanVerify => IsConfigured && IncludesDatabase && !IsRunning;
    public string SecretsKeyLocation { get; } = secretsKey.Location;
    public bool SecretsKeyAvailable { get; } = secretsKey.IsAvailable;

    public DateTime Local(DateTime utc)
    {
        return SiteTime.FromUtc(utc, timeZone);
    }
}
