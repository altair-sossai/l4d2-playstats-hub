using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Administrators.Services;

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
