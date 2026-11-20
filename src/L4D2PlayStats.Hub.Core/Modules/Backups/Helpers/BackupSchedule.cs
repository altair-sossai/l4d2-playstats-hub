using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;

public static class BackupSchedule
{
    public static bool IsDue(BackupSettings settings, BackupHistory history, DateTime nowUtc)
    {
        var next = NextAtUtc(settings, history);

        return next != null && nowUtc >= next.Value;
    }

    public static DateTime? NextAtUtc(BackupSettings settings, BackupHistory history)
    {
        if (!settings.Enabled || !BackupRules.IsConfigured(settings))
            return null;

        var lastSuccess = history.Runs.Where(run => run.Succeeded).Select(run => (DateTime?)run.StartedAtUtc).Max();
        var latest = history.Runs.OrderByDescending(run => run.StartedAtUtc).FirstOrDefault();
        var due = lastSuccess == null ? DateTime.MinValue : lastSuccess.Value.AddHours(BackupRules.Interval(settings.IntervalInHours));

        if (latest is not { Succeeded: false })
            return due;

        var retry = latest.StartedAtUtc.AddMinutes(AppConsts.BackupRetryInMinutes);

        return retry > due ? retry : due;
    }
}