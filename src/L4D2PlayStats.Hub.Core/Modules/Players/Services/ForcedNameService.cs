using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Enums;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class ForcedNameService(HubDbContext dbContext, HubState hubState, IClock clock, IForcedNameStore store) : IForcedNameService
{
    public async Task<Page<ForcedNameEntry>> PageAsync(PlayerSearch search, int page, int size, CancellationToken cancellationToken = default)
    {
        var query = search
            .Apply(dbContext.Set<ForcedName>().AsNoTracking(), entry => entry.CommunityId, entry => entry.Name)
            .OrderBy(entry => entry.Name.ToLower())
            .ThenBy(entry => entry.CommunityId);

        var entries = await query.ToPageAsync(page, size, cancellationToken);
        List<long> ids = [.. entries.Items.Select(entry => entry.CommunityId)];

        var current = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(profile => ids.Contains(profile.CommunityId) && profile.Name != null)
            .ToDictionaryAsync(profile => profile.CommunityId, profile => profile.Name, cancellationToken);

        return entries.Map(entry => new ForcedNameEntry(entry.CommunityId, entry.Name, current.GetValueOrDefault(entry.CommunityId), entry.UpdatedAtUtc));
    }

    public async Task<ForcedNameSaveResult> SaveAsync(long communityId, string? name, long? updatedBy, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId))
            return ForcedNameSaveResult.InvalidPlayer;

        var clean = Text.Clip(name, AppConsts.ForcedNameMaxLength);

        if (clean == null)
            return ForcedNameSaveResult.InvalidName;

        await using (await hubState.LockAsync(cancellationToken))
        {
            await UpsertAsync(communityId, clean, updatedBy, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await LoadAsync(cancellationToken);
        }

        Changed();

        return ForcedNameSaveResult.Saved;
    }

    public async Task<bool> DeleteAsync(long communityId, CancellationToken cancellationToken = default)
    {
        int removed;

        await using (await hubState.LockAsync(cancellationToken))
        {
            removed = await dbContext.Set<ForcedName>().Where(entry => entry.CommunityId == communityId).ExecuteDeleteAsync(cancellationToken);

            if (removed > 0)
                await LoadAsync(cancellationToken);
        }

        if (removed > 0)
            Changed();

        return removed > 0;
    }

    public async Task<ForcedNameImport> ImportAsync(string? text, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var parsed = new Dictionary<long, string>();
        var invalid = new List<string>();

        foreach (var line in (text ?? string.Empty).Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var match = Regex.Match(line, AppConsts.ForcedNameImportPattern);

            if (!match.Success)
                continue;

            var communityId = SteamIdParser.Parse(match.Groups[1].Value);
            var name = Text.Clip(match.Groups[2].Value, AppConsts.ForcedNameMaxLength);

            if (communityId == null || name == null)
            {
                invalid.Add(line);
                continue;
            }

            parsed[communityId.Value] = name;
        }

        int added = 0, updated = 0, unchanged = 0;

        await using (await hubState.LockAsync(cancellationToken))
        {
            List<long> ids = [.. parsed.Keys];
            var existing = await dbContext.Set<ForcedName>().Where(entry => ids.Contains(entry.CommunityId)).ToDictionaryAsync(entry => entry.CommunityId, cancellationToken);

            foreach (var (communityId, name) in parsed)
            {
                if (!existing.TryGetValue(communityId, out var entry))
                {
                    dbContext.Add(new ForcedName { CommunityId = communityId, Name = name, UpdatedAtUtc = clock.UtcNow, UpdatedBy = updatedBy });
                    added++;
                }
                else if (entry.Name != name)
                {
                    entry.Name = name;
                    entry.UpdatedAtUtc = clock.UtcNow;
                    entry.UpdatedBy = updatedBy;
                    updated++;
                }
                else
                {
                    unchanged++;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            if (added + updated > 0)
                await LoadAsync(cancellationToken);
        }

        if (added + updated > 0)
            Changed();

        return new ForcedNameImport(added, updated, unchanged, invalid);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var names = await dbContext.Set<ForcedName>().AsNoTracking().ToDictionaryAsync(entry => entry.CommunityId, entry => entry.Name, cancellationToken);

        store.Replace(names);
    }

    private async Task UpsertAsync(long communityId, string name, long? updatedBy, CancellationToken cancellationToken)
    {
        var entry = await dbContext.Set<ForcedName>().FirstOrDefaultAsync(e => e.CommunityId == communityId, cancellationToken);

        if (entry == null)
        {
            dbContext.Add(new ForcedName { CommunityId = communityId, Name = name, UpdatedAtUtc = clock.UtcNow, UpdatedBy = updatedBy });
            return;
        }

        entry.Name = name;
        entry.UpdatedAtUtc = clock.UtcNow;
        entry.UpdatedBy = updatedBy;
    }

    private void Changed()
    {
        hubState.DataChanged();
        hubState.CommunityChanged();
    }
}
