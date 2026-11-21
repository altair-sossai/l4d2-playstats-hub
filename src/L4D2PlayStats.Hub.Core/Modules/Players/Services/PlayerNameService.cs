using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerNameService(HubDbContext dbContext, IForcedNameStore forcedNames) : IPlayerNameService
{
    public async Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return forcedNames.Find(communityId) ?? await RealNameAsync(communityId, cancellationToken);
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

    public async Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        var ids = communityIds.Distinct().ToList();
        var missing = ids.Where(id => forcedNames.Find(id) == null).ToList();

        var profiles = missing.Count == 0
            ? []
            : await dbContext.PlayerProfiles
                .AsNoTracking()
                .Where(p => missing.Contains(p.CommunityId) && p.Name != null && p.Name != string.Empty)
                .ToDictionaryAsync(p => p.CommunityId, p => p.Name!, cancellationToken);

        List<long> unnamed = [.. missing.Where(id => string.IsNullOrWhiteSpace(profiles.GetValueOrDefault(id)))];

        var connections = unnamed.Count == 0
            ? []
            : await dbContext.Set<PlayerConnection>()
                .AsNoTracking()
                .Where(c => unnamed.Contains(c.CommunityId) && c.Name != string.Empty)
                .Select(c => new { c.CommunityId, c.Name, c.LastSeenAtUtc })
                .ToListAsync(cancellationToken);

        var latest = connections
            .Where(c => !string.IsNullOrWhiteSpace(c.Name))
            .GroupBy(c => c.CommunityId)
            .ToDictionary(group => group.Key, group => group.MaxBy(c => c.LastSeenAtUtc)!.Name);

        return ids.ToDictionary(id => id, id => forcedNames.Find(id) ?? Named(profiles.GetValueOrDefault(id)) ?? latest.GetValueOrDefault(id) ?? id.ToString(CultureInfo.InvariantCulture));
    }

    private static string? Named(string? name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : name;
    }
}
