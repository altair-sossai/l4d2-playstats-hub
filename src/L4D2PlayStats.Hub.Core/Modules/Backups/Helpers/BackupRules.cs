using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;

public static class BackupRules
{
    public static int Interval(int hours)
    {
        return Math.Clamp(hours, AppConsts.BackupMinIntervalInHours, AppConsts.BackupMaxIntervalInHours);
    }

    public static int Keep(int count)
    {
        return Math.Clamp(count, AppConsts.BackupMinKeep, AppConsts.BackupMaxKeep);
    }

    public static bool IsValidPrefix(string? prefix)
    {
        return !string.IsNullOrWhiteSpace(prefix) && prefix.Length <= AppConsts.BackupPrefixMaxLength && Regex.IsMatch(prefix, AppConsts.BackupPrefixPattern);
    }

    public static bool IsValidContainerUrl(string? address)
    {
        return Uri.TryCreate(address?.Trim(), UriKind.Absolute, out var uri)
               && uri.Scheme == Uri.UriSchemeHttps
               && uri.AbsolutePath.Trim('/').Length > 0
               && uri.Query.Contains(AppConsts.BackupSignatureParameter, StringComparison.Ordinal);
    }

    public static bool IsValidContainerName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name) && Regex.IsMatch(name, AppConsts.BackupContainerNamePattern);
    }

    public static bool IsConfigured(BackupSettings settings)
    {
        return settings.Destination == BackupDestination.ContainerUrl
            ? IsValidContainerUrl(settings.ContainerUrl)
            : !string.IsNullOrWhiteSpace(settings.ConnectionString) && IsValidContainerName(settings.ContainerName);
    }
}