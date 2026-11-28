using System.Text.RegularExpressions;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Helpers;

public static partial class OriginalCampaigns
{
    public static bool IsOriginal(IReadOnlyCollection<string> maps)
    {
        return maps.Count > 0 && maps.All(map => OriginalMap().IsMatch(map));
    }

    [GeneratedRegex(@"^c\d{1,2}m\d+_", RegexOptions.IgnoreCase)]
    private static partial Regex OriginalMap();
}
