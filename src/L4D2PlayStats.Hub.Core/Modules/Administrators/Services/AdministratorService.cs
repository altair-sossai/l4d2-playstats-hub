using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Administrators.Services;

public class AdministratorService(HubDbContext dbContext, IClock clock) : IAdministratorService
{
    public Task<bool> IsAdministratorAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return dbContext.Administrators.AnyAsync(a => a.CommunityId == communityId, cancellationToken);
    }

    public Task<List<Administrator>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Administrators.AsNoTracking().OrderBy(a => a.AddedAtUtc).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(long communityId, string? name, CancellationToken cancellationToken = default)
    {
        var administrator = await dbContext.Administrators.FirstOrDefaultAsync(a => a.CommunityId == communityId, cancellationToken);

        if (administrator == null)
        {
            administrator = new Administrator { CommunityId = communityId, AddedAtUtc = clock.UtcNow };
            dbContext.Administrators.Add(administrator);
        }

        if (!string.IsNullOrWhiteSpace(name))
            administrator.Name = name;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var count = await dbContext.Administrators.CountAsync(cancellationToken);
        if (count <= 1)
            return false;

        var administrator = await dbContext.Administrators.FirstOrDefaultAsync(a => a.CommunityId == communityId, cancellationToken);
        if (administrator == null)
            return false;

        dbContext.Administrators.Remove(administrator);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
