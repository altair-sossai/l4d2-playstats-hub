using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Models;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace L4D2PlayStats.Hub.Core.Modules.Campaigns.Services;

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

    public Task<List<Campaign>> ListAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return dbContext.Campaigns.AsNoTracking().Include(c => c.Maps).OrderBy(c => c.Id).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Campaigns.CountAsync(cancellationToken);
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
            dbContext.Campaigns.Add(NewCampaign(name.Trim(), maps));
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
