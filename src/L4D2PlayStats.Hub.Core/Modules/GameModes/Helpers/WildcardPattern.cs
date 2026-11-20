using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes.Helpers;

public static class WildcardPattern
{
    public static bool IsMatch(string pattern, string value)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return false;

        var expression = "^" + Regex.Escape(pattern.Trim()).Replace("\\*", ".*").Replace("\\?", ".") + "$";

        return Regex.IsMatch(value.Trim(), expression, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}