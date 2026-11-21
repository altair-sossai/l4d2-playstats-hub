using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Enums;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class ForcedNameService(HubDbContext dbContext, HubState hubState, IClock clock, IForcedNameStore store) : IForcedNameService
{
    public async Task<IReadOnlyList<ForcedNameEntry>> ListAsync(string? search, int skip, int take, CancellationToken cancellationToken = default)
    {
        var entries = await FilteredAsync(search, cancellationToken);
        var page = entries.Skip(Math.Max(0, skip)).Take(Math.Max(0, take)).ToList();
        List<long> ids = [.. page.Select(entry => entry.CommunityId)];

        var current = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(profile => ids.Contains(profile.CommunityId) && profile.Name != null)
            .ToDictionaryAsync(profile => profile.CommunityId, profile => profile.Name, cancellationToken);

        return [.. page.Select(entry => new ForcedNameEntry(entry.CommunityId, entry.Name, current.GetValueOrDefault(entry.CommunityId), entry.UpdatedAtUtc))];
    }

    public async Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
    {
        var entries = await FilteredAsync(search, cancellationToken);

        return entries.Count;
    }

    public async Task<ForcedNameSaveResult> SaveAsync(long communityId, string? name, long? updatedBy, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId))
            return ForcedNameSaveResult.InvalidPlayer;

        var clean = Clean(name);

        if (clean == null)
            return ForcedNameSaveResult.InvalidName;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            await UpsertAsync(communityId, clean, updatedBy, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await LoadAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        Changed();

        return ForcedNameSaveResult.Saved;
    }

    public async Task<bool> DeleteAsync(long communityId, CancellationToken cancellationToken = default)
    {
        int removed;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            removed = await dbContext.Set<ForcedName>().Where(entry => entry.CommunityId == communityId).ExecuteDeleteAsync(cancellationToken);

            if (removed > 0)
                await LoadAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
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
            var name = Clean(match.Groups[2].Value);

            if (communityId == null || name == null)
            {
                invalid.Add(line);
                continue;
            }

            parsed[communityId.Value] = name;
        }

        int added = 0, updated = 0, unchanged = 0;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
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
        finally
        {
            hubState.WriteLock.Release();
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

    private async Task<List<ForcedName>> FilteredAsync(string? search, CancellationToken cancellationToken)
    {
        var entries = await dbContext.Set<ForcedName>().AsNoTracking().ToListAsync(cancellationToken);
        var term = search?.Trim();
        var steamId = SteamIdParser.Parse(term);

        return
        [
            .. entries
                .Where(entry => string.IsNullOrEmpty(term)
                                || entry.CommunityId == steamId
                                || entry.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase)
                                || entry.CommunityId.ToString().Contains(term, StringComparison.Ordinal))
                .OrderBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(entry => entry.CommunityId)
        ];
    }

    private void Changed()
    {
        hubState.DataChanged();
        hubState.CommunityChanged();
    }

    private static string? Clean(string? name)
    {
        var clean = name?.Trim();

        if (string.IsNullOrEmpty(clean))
            return null;

        return clean.Length > AppConsts.ForcedNameMaxLength ? clean[..AppConsts.ForcedNameMaxLength] : clean;
    }
}
