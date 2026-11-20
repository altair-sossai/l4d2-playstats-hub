using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public class SkinService(HubDbContext dbContext, IClock clock) : ISkinService
{
    public Task<List<Skin>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Query().OrderBy(skin => skin.Id).ToListAsync(cancellationToken);
    }

    public Task<Skin?> FindAsync(string? skinId, CancellationToken cancellationToken = default)
    {
        var id = skinId?.Trim();

        return string.IsNullOrEmpty(id) ? Task.FromResult<Skin?>(null) : Query().FirstOrDefaultAsync(skin => skin.Id == id, cancellationToken);
    }

    public async Task<bool> SetPriceAsync(string? skinId, int? price, long? updatedBy, CancellationToken cancellationToken = default)
    {
        if (price is < AppConsts.SkinMinPrice or > AppConsts.SkinMaxPrice)
            return false;

        var skin = await FindAsync(skinId, cancellationToken);
        if (skin == null)
            return false;

        var priceOverride = await dbContext.Set<SkinPriceOverride>().FirstOrDefaultAsync(p => p.SkinId == skin.Id, cancellationToken);

        if (price == null || price == skin.DefaultPrice)
        {
            if (priceOverride != null)
                dbContext.Remove(priceOverride);

            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }

        if (priceOverride == null)
        {
            priceOverride = new SkinPriceOverride { SkinId = skin.Id };
            dbContext.Add(priceOverride);
        }

        priceOverride.Price = price.Value;
        priceOverride.UpdatedByCommunityId = updatedBy;
        priceOverride.UpdatedAtUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private IQueryable<Skin> Query()
    {
        return dbContext.Set<Skin>()
            .AsNoTracking()
            .Include(skin => skin.Texts)
            .Include(skin => skin.PriceOverride)
            .AsSplitQuery();
    }
}
