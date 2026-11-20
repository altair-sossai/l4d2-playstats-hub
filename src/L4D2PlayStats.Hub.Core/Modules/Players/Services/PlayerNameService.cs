using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerNameService(HubDbContext dbContext) : IPlayerNameService
{
    public async Task<string?> KnownNameAsync(long communityId, CancellationToken cancellationToken = default)
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
}