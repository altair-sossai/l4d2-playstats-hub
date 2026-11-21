using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerRegistryService(HubDbContext dbContext, IClock clock, IDisplayNameStore displayNames) : IPlayerRegistryService
{
    public async Task<PlayerRegistryBackfill> BackfillAsync(CancellationToken cancellationToken = default)
    {
        var created = await CreateMissingAsync(cancellationToken);
        var usages = await UsagesAsync(dbContext.MatchPlayers.AsNoTracking(), cancellationToken);
        var profiles = await dbContext.PlayerProfiles.ToListAsync(cancellationToken);

        profiles.AddRange(dbContext.PlayerProfiles.Local.Where(profile => dbContext.Entry(profile).State == EntityState.Added));

        var updated = Apply(profiles, usages);

        await dbContext.SaveChangesAsync(cancellationToken);

        displayNames.Replace(profiles.Select(profile => KeyValuePair.Create(profile.CommunityId, profile.DisplayName)));

        return new PlayerRegistryBackfill(created, updated.Count, profiles.Count);
    }

    public async Task<bool> RefreshAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        var changed = new List<PlayerProfile>();

        foreach (var chunk in communityIds.Where(SteamIds.IsValid).Distinct().Chunk(AppConsts.PlayerNameRefreshBatchSize))
        {
            List<long> ids = [.. chunk];
            var usages = await UsagesAsync(dbContext.MatchPlayers.AsNoTracking().Where(player => ids.Contains(player.CommunityId)), cancellationToken);
            var profiles = await dbContext.PlayerProfiles.Where(profile => ids.Contains(profile.CommunityId)).ToListAsync(cancellationToken);

            changed.AddRange(Apply(profiles, usages));
        }

        if (changed.Count == 0)
            return false;

        await dbContext.SaveChangesAsync(cancellationToken);

        Publish(changed);

        return true;
    }

    public async Task<bool> SeenAsync(long communityId, string? name, CancellationToken cancellationToken = default)
    {
        if (!SteamIds.IsValid(communityId))
            return false;

        var clean = Text.Clip(name, AppConsts.PlayerNameColumnMaxLength);
        var profile = await dbContext.PlayerProfiles.FirstOrDefaultAsync(p => p.CommunityId == communityId, cancellationToken);

        if (profile != null && (clean == null || profile.Name == clean))
            return false;

        if (profile == null)
        {
            var now = clock.UtcNow;

            profile = new PlayerProfile { CommunityId = communityId, FirstSeenAtUtc = now, LastSeenAtUtc = now };
            dbContext.PlayerProfiles.Add(profile);
        }

        profile.Name = clean;

        var renamed = PlayerDisplayNames.Refresh(profile);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.Entry(profile).State = EntityState.Detached;

            return false;
        }

        Publish([profile]);

        return renamed;
    }

    public void Publish(IEnumerable<PlayerProfile> profiles)
    {
        foreach (var profile in profiles)
            displayNames.Set(profile.CommunityId, profile.DisplayName);
    }

    private static List<PlayerProfile> Apply(IEnumerable<PlayerProfile> profiles, IReadOnlyList<PlayerNameUsage> usages)
    {
        var byPlayer = usages.ToLookup(usage => usage.CommunityId);
        var changed = new List<PlayerProfile>();

        foreach (var profile in profiles)
        {
            var mostUsed = MostUsedNames.Pick(byPlayer[profile.CommunityId]);
            var updated = mostUsed != profile.MostUsedName;

            profile.MostUsedName = mostUsed;

            if (PlayerDisplayNames.Refresh(profile) || updated)
                changed.Add(profile);
        }

        return changed;
    }

    private static Task<List<PlayerNameUsage>> UsagesAsync(IQueryable<MatchPlayer> players, CancellationToken cancellationToken)
    {
        return players
            .Where(player => player.Name != null && player.Name != string.Empty)
            .GroupBy(player => new { player.CommunityId, player.Name })
            .Select(group => new PlayerNameUsage(group.Key.CommunityId, group.Key.Name!, group.Count(), group.Min(player => player.Id)))
            .ToListAsync(cancellationToken);
    }

    private async Task<int> CreateMissingAsync(CancellationToken cancellationToken)
    {
        var profiles = dbContext.PlayerProfiles;

        var played = await dbContext.MatchPlayers
            .AsNoTracking()
            .Where(player => player.CommunityId > AppConsts.SteamCommunityIdBase && !profiles.Any(profile => profile.CommunityId == player.CommunityId))
            .Select(player => new { player.Id, player.CommunityId, player.Name, When = player.Match!.EndedAtUtc ?? player.Match.LastFileDate })
            .ToListAsync(cancellationToken);

        var connected = await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(connection => connection.CommunityId > AppConsts.SteamCommunityIdBase && !profiles.Any(profile => profile.CommunityId == connection.CommunityId))
            .Select(connection => new { connection.CommunityId, connection.Name, connection.FirstSeenAtUtc, connection.LastSeenAtUtc })
            .ToListAsync(cancellationToken);

        var created = new Dictionary<long, PlayerProfile>();

        foreach (var group in played.GroupBy(player => player.CommunityId))
        {
            var latest = group.Where(player => !string.IsNullOrWhiteSpace(player.Name)).OrderByDescending(player => player.When).ThenByDescending(player => player.Id).FirstOrDefault();

            created[group.Key] = new PlayerProfile
            {
                CommunityId = group.Key,
                Name = Text.Clip(latest?.Name, AppConsts.PlayerNameColumnMaxLength),
                FirstSeenAtUtc = group.Min(player => player.When),
                LastSeenAtUtc = group.Max(player => player.When)
            };
        }

        foreach (var group in connected.GroupBy(connection => connection.CommunityId))
        {
            var latest = group.OrderByDescending(connection => connection.LastSeenAtUtc).First();

            if (created.TryGetValue(group.Key, out var profile))
            {
                profile.Name ??= Text.Clip(latest.Name, AppConsts.PlayerNameColumnMaxLength);
                continue;
            }

            created[group.Key] = new PlayerProfile
            {
                CommunityId = group.Key,
                Name = Text.Clip(latest.Name, AppConsts.PlayerNameColumnMaxLength),
                FirstSeenAtUtc = group.Min(connection => connection.FirstSeenAtUtc),
                LastSeenAtUtc = latest.LastSeenAtUtc
            };
        }

        dbContext.PlayerProfiles.AddRange(created.Values);

        return created.Count;
    }
}
