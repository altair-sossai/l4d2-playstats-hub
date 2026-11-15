using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class SkinDisplay
{
    public static readonly SkinRarity[] Rarities = [SkinRarity.Legendary, SkinRarity.Epic, SkinRarity.Rare, SkinRarity.Uncommon, SkinRarity.Common];

    public static string RarityCss(SkinRarity rarity)
    {
        return $"skin-rarity-{rarity.ToString().ToLowerInvariant()}";
    }

    public static string? WorkshopUrl(Skin skin)
    {
        return string.IsNullOrEmpty(skin.WorkshopId) ? null : $"{CoreConsts.SkinWorkshopUrl}{Uri.EscapeDataString(skin.WorkshopId)}";
    }

    public static object[] Arguments(string values)
    {
        return [.. values.Split(AppConsts.SkinTempDataSeparator).Cast<object>()];
    }

    public static string Query(params (string Name, object? Value)[] parameters)
    {
        var parts = parameters
            .Where(parameter => parameter.Value != null && !string.IsNullOrEmpty(parameter.Value.ToString()))
            .Select(parameter => $"{parameter.Name}={Uri.EscapeDataString(parameter.Value!.ToString()!)}")
            .ToList();

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }
}
