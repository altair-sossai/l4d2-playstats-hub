using L4D2PlayStats.Hub.Core.Modules.Backups.Enums;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;

public static class BackupSecrets
{
    private static readonly string[] SecretPrefixes = [AppConsts.BackupConnectionKeyPrefix, AppConsts.BackupConnectionSignaturePrefix];

    public static string? Mask(BackupSettings settings)
    {
        return settings.Destination == BackupDestination.ContainerUrl ? MaskContainerUrl(settings.ContainerUrl) : MaskConnectionString(settings.ConnectionString);
    }

    public static string? MaskContainerUrl(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        return Uri.TryCreate(address.Trim(), UriKind.Absolute, out var uri)
            ? $"{uri.GetLeftPart(UriPartial.Path)}?{AppConsts.BackupSecretMask}"
            : AppConsts.BackupSecretMask;
    }

    public static string? MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        var parts = Parts(connectionString).Select(MaskPart);

        return string.Join(AppConsts.BackupConnectionSeparator, parts);
    }

    public static string Redact(string text, BackupSettings settings)
    {
        var redacted = text;

        foreach (var secret in Secrets(settings).Where(secret => secret.Length > 0).OrderByDescending(secret => secret.Length))
            redacted = redacted.Replace(secret, AppConsts.BackupSecretMask, StringComparison.Ordinal);

        return redacted;
    }

    private static IEnumerable<string> Parts(string connectionString)
    {
        return connectionString.Trim()
            .Split(AppConsts.BackupConnectionSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim());
    }

    private static string MaskPart(string part)
    {
        var prefix = SecretPrefixes.FirstOrDefault(candidate => part.StartsWith(candidate, StringComparison.OrdinalIgnoreCase));

        return prefix == null ? part : part[..prefix.Length] + AppConsts.BackupSecretMask;
    }

    private static IEnumerable<string> Secrets(BackupSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.ContainerUrl))
        {
            var address = settings.ContainerUrl.Trim();
            var query = address.IndexOf('?');

            yield return address;

            if (query >= 0)
                yield return address[(query + 1)..];
        }

        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            yield break;

        yield return settings.ConnectionString.Trim();

        foreach (var part in Parts(settings.ConnectionString))
        {
            var prefix = SecretPrefixes.FirstOrDefault(candidate => part.StartsWith(candidate, StringComparison.OrdinalIgnoreCase));

            if (prefix != null)
                yield return part[prefix.Length..];
        }
    }
}