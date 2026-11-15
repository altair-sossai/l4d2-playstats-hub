using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Administrators;

public class Administrator
{
    public long CommunityId { get; set; }
    public string? Name { get; set; }
    public DateTime AddedAtUtc { get; set; }
}

public interface IAdministratorService
{
    Task<bool> IsAdministratorAsync(long communityId, CancellationToken cancellationToken = default);
    Task<List<Administrator>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(long communityId, string? name, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(long communityId, CancellationToken cancellationToken = default);
}

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

public interface ISetupService
{
    Task<bool> IsPendingAsync(CancellationToken cancellationToken = default);
    Task<string?> EnsureTokenAsync(CancellationToken cancellationToken = default);
    Task<bool> IsValidTokenAsync(string? token, CancellationToken cancellationToken = default);
    Task<bool> ClaimAsync(string? token, long communityId, string? name, CancellationToken cancellationToken = default);
    Task CompleteAsync(CancellationToken cancellationToken = default);
}

public class SetupService(HubDbContext dbContext, ISettingsService settingsService, IAdministratorService administratorService) : ISetupService
{
    public async Task<bool> IsPendingAsync(CancellationToken cancellationToken = default)
    {
        var setup = await settingsService.GetAsync<SetupSettings>(cancellationToken);

        return !setup.Completed;
    }

    public async Task<string?> EnsureTokenAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Administrators.AnyAsync(cancellationToken))
            return null;

        var setup = await settingsService.GetAsync<SetupSettings>(cancellationToken);
        if (!string.IsNullOrEmpty(setup.Token))
            return setup.Token;

        setup.Token = SecretGenerator.NewSecret(AppConsts.SetupTokenBytes);
        await settingsService.SaveAsync(setup, cancellationToken);

        return setup.Token;
    }

    public async Task<bool> IsValidTokenAsync(string? token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        if (await dbContext.Administrators.AnyAsync(cancellationToken))
            return false;

        var setup = await settingsService.GetAsync<SetupSettings>(cancellationToken);

        return !string.IsNullOrEmpty(setup.Token) && SecretGenerator.Matches(token.Trim(), SecretGenerator.Hash(setup.Token));
    }

    public async Task<bool> ClaimAsync(string? token, long communityId, string? name, CancellationToken cancellationToken = default)
    {
        if (!await IsValidTokenAsync(token, cancellationToken))
            return false;

        await administratorService.AddAsync(communityId, name, cancellationToken);

        var setup = await settingsService.GetAsync<SetupSettings>(cancellationToken);
        setup.Token = null;
        await settingsService.SaveAsync(setup, cancellationToken);

        return true;
    }

    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        var setup = await settingsService.GetAsync<SetupSettings>(cancellationToken);
        setup.Completed = true;
        await settingsService.SaveAsync(setup, cancellationToken);
    }
}
