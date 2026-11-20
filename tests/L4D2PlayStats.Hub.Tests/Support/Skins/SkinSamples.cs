using System.Reflection;
using System.Text.Json.Nodes;

namespace L4D2PlayStats.Hub.Tests.Support.Skins;

public static class SkinSamples
{
    public const string CatalogFileName = "skins.json";
    public const string DesertSmg = "desert-smg";
    public const string NeonSmg = "neon-smg";
    public const string GoldMagnum = "gold-magnum";
    public const string DragonKatana = "dragon-katana";
    public const string GalaxyPump = "galaxy-pump";
    public const string Smg = "weapon_smg";
    public const string Magnum = "weapon_pistol_magnum";
    public const int Count = 5;

    public static string Catalog()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames().First(name => name.EndsWith(CatalogFileName, StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static string Catalog(Action<JsonArray> change)
    {
        var catalog = JsonNode.Parse(Catalog())!;
        var skins = catalog["skins"]!.AsArray();

        change(skins);

        return catalog.ToJsonString();
    }

    public static JsonObject Find(JsonArray skins, string id)
    {
        return skins.Select(skin => skin!.AsObject()).First(skin => skin["id"]!.GetValue<string>() == id);
    }

    public static JsonObject NewSkin(string id, string weapon, int price)
    {
        return new JsonObject
        {
            ["id"] = id,
            ["weapon"] = weapon,
            ["rarity"] = "uncommon",
            ["price"] = price,
            ["workshopId"] = "1000000099",
            ["shots"] = 0,
            ["author"] = new JsonObject { ["name"] = "New Author", ["communityId"] = "76561198000000003" },
            ["title"] = new JsonObject { ["en"] = $"Skin {id}" },
            ["description"] = new JsonObject { ["en"] = $"Description of {id}" }
        };
    }
}
