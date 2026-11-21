using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

public static class MostUsedNames
{
    public static string? Pick(IEnumerable<PlayerNameUsage> usages)
    {
        return Rank(usages).FirstOrDefault();
    }

    public static IEnumerable<string> Rank(IEnumerable<PlayerNameUsage> usages)
    {
        return usages
            .Select(usage => new { Name = Text.Clip(usage.Name, AppConsts.PlayerNameColumnMaxLength), usage.Times, usage.FirstId })
            .Where(usage => usage.Name != null)
            .GroupBy(usage => usage.Name!, StringComparer.CurrentCultureIgnoreCase)
            .Select(group => new { group.MinBy(usage => usage.FirstId)!.Name, Times = group.Sum(usage => usage.Times), First = group.Min(usage => usage.FirstId) })
            .OrderByDescending(name => name.Times)
            .ThenBy(name => name.First)
            .Select(name => name.Name!);
    }
}
