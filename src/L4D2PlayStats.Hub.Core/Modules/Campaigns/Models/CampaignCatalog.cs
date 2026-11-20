using System.Reflection;
using System.Text.Json;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Models;

public class CampaignCatalog(IReadOnlyList<CampaignDefinition> campaigns)
{
    private readonly HashSet<string> _maps = [.. campaigns.SelectMany(c => c.Maps)];

    public IReadOnlyList<CampaignDefinition> Campaigns => campaigns;

    public bool IsKnownMap(string mapName)
    {
        return _maps.Contains(mapName);
    }

    public CampaignDefinition? FindByName(string? name)
    {
        return name == null ? null : campaigns.FirstOrDefault(c => c.Name == name);
    }

    public CampaignDefinition? FindUsingMapName(string mapName)
    {
        return campaigns
            .Where(c => c.IndexOf(mapName) != -1)
            .MaxBy(c => c.IndexOf(mapName));
    }

    public static CampaignCatalog Default()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource = assembly.GetManifestResourceNames().First(name => name.EndsWith("campaigns.json", StringComparison.Ordinal));

        using var stream = assembly.GetManifestResourceStream(resource)!;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var definitions = JsonSerializer.Deserialize<List<CampaignSeed>>(stream, options) ?? [];

        return new CampaignCatalog([.. definitions.Select(d => new CampaignDefinition(d.Name, d.Maps))]);
    }

    private record CampaignSeed(string Name, List<string> Maps);
}
