using System.Reflection;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns;

public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<CampaignMap> Maps { get; set; } = [];
}

public class CampaignMap
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string MapName { get; set; } = null!;
    public int Order { get; set; }
}

public record CampaignDefinition(string Name, IReadOnlyList<string> Maps)
{
    public bool SequentialMaps(string current, string next)
    {
        var currentIndex = IndexOf(current);
        if (currentIndex == -1)
            return false;

        var nextIndex = IndexOf(next);
        if (nextIndex == -1)
            return false;

        return currentIndex == nextIndex - 1;
    }

    public int IndexOf(string mapName)
    {
        for (var index = 0; index < Maps.Count; index++)
            if (Maps[index] == mapName)
                return index;

        return -1;
    }
}

public class CampaignCatalog(IReadOnlyList<CampaignDefinition> campaigns)
{
    private readonly HashSet<string> _maps = campaigns.SelectMany(c => c.Maps).ToHashSet();

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

public interface ICampaignService
{
    Task<CampaignCatalog> GetCatalogAsync(CancellationToken cancellationToken = default);
    Task<List<Campaign>> ListAsync(CancellationToken cancellationToken = default);
    Task SeedAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(int? id, string name, IReadOnlyList<string> maps, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class CampaignService(HubDbContext dbContext, IMemoryCache memoryCache, HubState hubState) : ICampaignService
{
    private const string CacheKey = "campaign_catalog";

    public async Task<CampaignCatalog> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await memoryCache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(AppConsts.SettingsCacheInMinutes);

            var campaigns = await ListAsync(cancellationToken);

            return new CampaignCatalog([.. campaigns.Select(c => new CampaignDefinition(c.Name, [.. c.Maps.OrderBy(m => m.Order).Select(m => m.MapName)]))]);
        });

        return catalog!;
    }

    public Task<List<Campaign>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Campaigns.AsNoTracking().Include(c => c.Maps).OrderBy(c => c.Id).ToListAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Campaigns.AnyAsync(cancellationToken))
            return;

        foreach (var definition in CampaignCatalog.Default().Campaigns)
            dbContext.Campaigns.Add(NewCampaign(definition.Name, definition.Maps));

        await dbContext.SaveChangesAsync(cancellationToken);
        memoryCache.Remove(CacheKey);
    }

    public async Task SaveAsync(int? id, string name, IReadOnlyList<string> maps, CancellationToken cancellationToken = default)
    {
        var campaign = id == null ? null : await dbContext.Campaigns.Include(c => c.Maps).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null)
        {
            dbContext.Campaigns.Add(NewCampaign(name.Trim(), maps));
        }
        else
        {
            campaign.Name = name.Trim();
            dbContext.CampaignMaps.RemoveRange(campaign.Maps);
            campaign.Maps = NewMaps(maps);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        memoryCache.Remove(CacheKey);
        hubState.DataChanged();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var campaign = await dbContext.Campaigns.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (campaign == null)
            return false;

        dbContext.Campaigns.Remove(campaign);
        await dbContext.SaveChangesAsync(cancellationToken);
        memoryCache.Remove(CacheKey);
        hubState.DataChanged();

        return true;
    }

    private static Campaign NewCampaign(string name, IReadOnlyList<string> maps)
    {
        return new Campaign { Name = name, Maps = NewMaps(maps) };
    }

    private static List<CampaignMap> NewMaps(IReadOnlyList<string> maps)
    {
        return [.. maps.Where(m => !string.IsNullOrWhiteSpace(m)).Select((mapName, index) => new CampaignMap { MapName = mapName.Trim(), Order = index })];
    }
}
