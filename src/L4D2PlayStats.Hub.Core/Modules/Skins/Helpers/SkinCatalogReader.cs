using System.Text.Json;
using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;

public static class SkinCatalogReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        Converters = { new JsonStringEnumConverter() }
    };

    public static List<SkinCatalogEntry>? Read(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var catalog = JsonSerializer.Deserialize<SkinCatalogFile>(json, JsonOptions);
            var skins = catalog?.Skins;

            if (skins == null || skins.Any(skin => skin == null))
                return null;

            foreach (var skin in skins)
            {
                skin.Id = skin.Id?.Trim();
                skin.Weapon = skin.Weapon?.Trim();
            }

            return IsValid(skins) ? skins : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool IsValid(List<SkinCatalogEntry> skins)
    {
        var ids = skins.Select(skin => skin.Id).ToHashSet();

        return ids.Count == skins.Count && skins.All(IsValid);
    }

    private static bool IsValid(SkinCatalogEntry skin)
    {
        return !string.IsNullOrEmpty(skin.Id)
               && skin.Id.Length <= AppConsts.SkinIdMaxLength
               && !string.IsNullOrEmpty(skin.Weapon)
               && skin.Weapon.Length <= AppConsts.SkinWeaponMaxLength
               && skin.Rarity != null
               && Enum.IsDefined(skin.Rarity.Value)
               && skin.Price is >= AppConsts.SkinMinPrice and <= AppConsts.SkinMaxPrice
               && skin.Shots >= 0;
    }
}