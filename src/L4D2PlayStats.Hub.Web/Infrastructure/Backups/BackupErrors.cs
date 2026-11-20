using L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Backups.Settings;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Backups;

public static class BackupErrors
{
    public static string Describe(Exception exception, BackupSettings settings)
    {
        var firstLine = exception.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? exception.GetType().Name;

        return Limit(BackupSecrets.Redact(firstLine, settings));
    }

    public static string Limit(string text)
    {
        return text.Length <= AppConsts.BackupErrorMaxLength ? text : text[..AppConsts.BackupErrorMaxLength];
    }
}