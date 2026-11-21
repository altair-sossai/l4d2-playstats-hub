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

public class ForcedNameService(HubDbContext dbContext, HubState hubState, IClock clock, IForcedNameStore store, IPlayerRegistryService playerRegistryService) : IForcedNameService
{
    public async Task<Page<ForcedNameEntry>> PageAsync(PlayerSearch search, int page, int size, CancellationToken cancellationToken = default)
    {
        var query = search
            .Apply(Forced().AsNoTracking(), profile => profile.CommunityId, profile => profile.ForcedName)
            .OrderBy(profile => profile.ForcedName!.ToLower())
            .ThenBy(profile => profile.CommunityId)
            .Select(profile => new ForcedNameEntry(profile.CommunityId, profile.ForcedName!, profile.Name, profile.ForcedNameUpdatedAtUtc ?? profile.LastSeenAtUtc));

        return await query.ToPageAsync(page, size, cancellationToken);
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
            var profiles = await ProfilesAsync([communityId], cancellationToken);
            var profile = Profile(profiles, communityId);

            Force(profile, clean, updatedBy);

            await SaveAsync([profile], cancellationToken);
        }

        Changed();

        return ForcedNameSaveResult.Saved;
    }

    public async Task<bool> DeleteAsync(long communityId, CancellationToken cancellationToken = default)
    {
        await using (await hubState.LockAsync(cancellationToken))
        {
            var profile = await Forced().FirstOrDefaultAsync(p => p.CommunityId == communityId, cancellationToken);

            if (profile == null)
                return false;

            profile.ForcedName = null;
            profile.ForcedNameUpdatedAtUtc = clock.UtcNow;
            profile.ForcedNameUpdatedBy = null;

            await SaveAsync([profile], cancellationToken);
        }

        Changed();

        return true;
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
            var profiles = await ProfilesAsync([.. parsed.Keys], cancellationToken);
            var changed = new List<PlayerProfile>();

            foreach (var (communityId, name) in parsed)
            {
                var profile = Profile(profiles, communityId);

                if (profile.ForcedName == name)
                {
                    unchanged++;
                    continue;
                }

                if (profile.ForcedName == null)
                    added++;
                else
                    updated++;

                Force(profile, name, updatedBy);
                changed.Add(profile);
            }

            if (changed.Count > 0)
                await SaveAsync(changed, cancellationToken);
        }

        if (added + updated > 0)
            Changed();

        return new ForcedNameImport(added, updated, unchanged, invalid);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var names = await Forced()
            .AsNoTracking()
            .Select(profile => new { profile.CommunityId, profile.ForcedName })
            .ToDictionaryAsync(entry => entry.CommunityId, entry => entry.ForcedName!, cancellationToken);

        store.Replace(names);
    }

    private IQueryable<PlayerProfile> Forced()
    {
        return dbContext.PlayerProfiles.Where(profile => profile.ForcedName != null);
    }

    private async Task<Dictionary<long, PlayerProfile>> ProfilesAsync(List<long> communityIds, CancellationToken cancellationToken)
    {
        var profiles = new Dictionary<long, PlayerProfile>();

        foreach (var chunk in communityIds.Chunk(AppConsts.PlayerNameRefreshBatchSize))
        {
            List<long> ids = [.. chunk];
            var found = await dbContext.PlayerProfiles.Where(profile => ids.Contains(profile.CommunityId)).ToListAsync(cancellationToken);

            foreach (var profile in found)
                profiles[profile.CommunityId] = profile;
        }

        return profiles;
    }

    private PlayerProfile Profile(Dictionary<long, PlayerProfile> profiles, long communityId)
    {
        if (profiles.TryGetValue(communityId, out var profile))
            return profile;

        var now = clock.UtcNow;

        profile = new PlayerProfile { CommunityId = communityId, FirstSeenAtUtc = now, LastSeenAtUtc = now };
        profiles[communityId] = profile;
        dbContext.PlayerProfiles.Add(profile);

        return profile;
    }

    private void Force(PlayerProfile profile, string name, long? updatedBy)
    {
        profile.ForcedName = name;
        profile.ForcedNameUpdatedAtUtc = clock.UtcNow;
        profile.ForcedNameUpdatedBy = updatedBy;
    }

    private async Task SaveAsync(IReadOnlyCollection<PlayerProfile> profiles, CancellationToken cancellationToken)
    {
        foreach (var profile in profiles)
            PlayerDisplayNames.Refresh(profile);

        await dbContext.SaveChangesAsync(cancellationToken);

        playerRegistryService.Publish(profiles);

        await LoadAsync(cancellationToken);
    }

    private void Changed()
    {
        hubState.DataChanged();
        hubState.CommunityChanged();
    }
}
