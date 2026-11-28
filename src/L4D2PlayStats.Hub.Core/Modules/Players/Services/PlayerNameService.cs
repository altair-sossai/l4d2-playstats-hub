using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerNameService(HubDbContext dbContext) : IPlayerNameService
{
    public async Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var displayName = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => p.CommunityId == communityId)
            .Select(p => p.DisplayName)
            .FirstOrDefaultAsync(cancellationToken);

        return PlayerDisplayNames.Known(communityId, displayName) ?? await RealNameAsync(communityId, cancellationToken);
    }

    public async Task<string?> RealNameAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => p.CommunityId == communityId && p.Name != null)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(profile))
            return profile;

        var connection = await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(c => c.CommunityId == communityId)
            .OrderByDescending(c => c.LastSeenAtUtc)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(connection) ? null : connection;
    }

    public async Task<Dictionary<long, string>> NamesWithAdministratorsAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        var ids = communityIds.Distinct().ToList();
        var names = await dbContext.Administrators.AsNoTracking().Where(administrator => ids.Contains(administrator.CommunityId) && administrator.Name != null).ToDictionaryAsync(administrator => administrator.CommunityId, administrator => administrator.Name!, cancellationToken);

        foreach (var (communityId, name) in await NamesAsync(ids.Where(id => !names.ContainsKey(id)).ToList(), cancellationToken))
            names[communityId] = name;

        return names;
    }

    public async Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        var ids = communityIds.Distinct().ToList();
        var names = new Dictionary<long, string>();

        foreach (var chunk in ids.Chunk(AppConsts.PlayerNameRefreshBatchSize))
        {
            List<long> batch = [.. chunk];

            var profiles = await dbContext.PlayerProfiles
                .AsNoTracking()
                .Where(p => batch.Contains(p.CommunityId))
                .Select(p => new { p.CommunityId, p.DisplayName, p.Name })
                .ToListAsync(cancellationToken);

            foreach (var profile in profiles)
            {
                var name = PlayerDisplayNames.Known(profile.CommunityId, profile.DisplayName) ?? Named(profile.Name);

                if (name != null)
                    names[profile.CommunityId] = name;
            }

            List<long> unnamed = [.. batch.Where(id => !names.ContainsKey(id))];

            if (unnamed.Count == 0)
                continue;

            var connections = await dbContext.Set<PlayerConnection>()
                .AsNoTracking()
                .Where(c => unnamed.Contains(c.CommunityId) && c.Name != string.Empty)
                .Select(c => new { c.CommunityId, c.Name, c.LastSeenAtUtc })
                .ToListAsync(cancellationToken);

            foreach (var group in connections.Where(c => !string.IsNullOrWhiteSpace(c.Name)).GroupBy(c => c.CommunityId))
                names[group.Key] = group.MaxBy(c => c.LastSeenAtUtc)!.Name;
        }

        return ids.ToDictionary(id => id, id => names.GetValueOrDefault(id) ?? PlayerDisplayNames.Fallback(id));
    }

    public async Task<Dictionary<long, string>> DisplayNamesAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => p.DisplayName != null)
            .Select(p => new { p.CommunityId, p.DisplayName })
            .ToListAsync(cancellationToken);

        return profiles
            .Select(p => new { p.CommunityId, Name = PlayerDisplayNames.Known(p.CommunityId, p.DisplayName) })
            .Where(p => p.Name != null)
            .ToDictionary(p => p.CommunityId, p => p.Name!);
    }

    private static string? Named(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }
}
