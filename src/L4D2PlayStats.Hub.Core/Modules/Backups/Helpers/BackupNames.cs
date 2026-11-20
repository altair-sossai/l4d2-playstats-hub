using System.Globalization;
using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Core.Modules.Backups.Helpers;

public static class BackupNames
{
    public static string FileName(string prefix, DateTime utc)
    {
        return string.Format(CultureInfo.InvariantCulture, AppConsts.BackupFileNameFormat, prefix, utc.ToString(AppConsts.BackupFileTimeFormat, CultureInfo.InvariantCulture), AppConsts.BackupFileExtension);
    }

    public static bool IsBackup(string prefix, string name)
    {
        return Regex.IsMatch(name, string.Format(CultureInfo.InvariantCulture, AppConsts.BackupNamePatternFormat, Regex.Escape(prefix)));
    }

    public static List<string> Expired(string prefix, IEnumerable<string> names, int keep)
    {
        return
        [
            .. names
                .Where(name => IsBackup(prefix, name))
                .OrderByDescending(name => name, StringComparer.Ordinal)
                .Skip(BackupRules.Keep(keep))
        ];
    }
}