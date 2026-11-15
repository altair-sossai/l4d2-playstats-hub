using System.Net;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class PlayerConnectionService(HubDbContext dbContext, IClock clock) : IPlayerConnectionService
{
    public async Task<List<PlayerConnectionResult>?> RegisterAsync(PlayerConnectionCommand? command, CancellationToken cancellationToken = default)
    {
        if (command == null || string.IsNullOrWhiteSpace(command.Name))
            return null;

        if (!long.TryParse(command.CommunityId, out var communityId) || !SteamIds.IsValid(communityId))
            return null;

        if (!IPAddress.TryParse(command.IpAddress, out var parsed))
            return null;

        var ipAddress = parsed.ToString();
        var name = command.Name.Trim();
        var now = clock.UtcNow;

        if (name.Length > AppConsts.LivePlayerNameMaxLength)
            name = name[..AppConsts.LivePlayerNameMaxLength];

        var connection = await dbContext.Set<PlayerConnection>().FirstOrDefaultAsync(c => c.IpAddress == ipAddress && c.CommunityId == communityId, cancellationToken);

        if (connection == null)
        {
            connection = new PlayerConnection { IpAddress = ipAddress, CommunityId = communityId, FirstSeenAtUtc = now };
            dbContext.Set<PlayerConnection>().Add(connection);
        }

        connection.Name = name;
        connection.LastSeenAtUtc = now;
        connection.Count++;

        await dbContext.SaveChangesAsync(cancellationToken);

        return await RelatedAsync(communityId, cancellationToken);
    }

    public async Task<List<PlayerConnectionResult>> RelatedAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var connections = await SameAddressAsync(communityId, cancellationToken);

        return Group(connections.Where(c => c.CommunityId != communityId));
    }

    public async Task<PlayerConnectionDetails> DetailsAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var connections = await SameAddressAsync(communityId, cancellationToken);
        var player = Group(connections.Where(c => c.CommunityId == communityId)).FirstOrDefault();

        return new PlayerConnectionDetails(player, Group(connections.Where(c => c.CommunityId != communityId)));
    }

    public async Task<int> CleanupAsync(CancellationToken cancellationToken = default)
    {
        var limit = clock.UtcNow.AddMonths(-AppConsts.PlayerConnectionRetentionInMonths);
        var deleted = 0;

        while (true)
        {
            var expired = await dbContext.Set<PlayerConnection>()
                .Where(c => c.LastSeenAtUtc < limit)
                .OrderBy(c => c.Id)
                .Take(AppConsts.PlayerConnectionCleanupBatchSize)
                .ToListAsync(cancellationToken);

            if (expired.Count == 0)
                return deleted;

            dbContext.Set<PlayerConnection>().RemoveRange(expired);
            await dbContext.SaveChangesAsync(cancellationToken);

            deleted += expired.Count;
        }
    }

    private async Task<List<PlayerConnection>> SameAddressAsync(long communityId, CancellationToken cancellationToken)
    {
        var addresses = await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(c => c.CommunityId == communityId)
            .Select(c => c.IpAddress)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (addresses.Count == 0)
            return [];

        return await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(c => addresses.Contains(c.IpAddress))
            .ToListAsync(cancellationToken);
    }

    private static List<PlayerConnectionResult> Group(IEnumerable<PlayerConnection> connections)
    {
        return
        [
            .. connections
                .GroupBy(c => c.CommunityId)
                .Select(group =>
                {
                    var latest = group.OrderByDescending(c => c.LastSeenAtUtc).First();

                    return new PlayerConnectionResult
                    {
                        CommunityId = latest.CommunityId.ToString(),
                        Name = latest.Name,
                        SteamId = SteamIds.SteamId(latest.CommunityId),
                        Steam3 = SteamIds.Steam3(latest.CommunityId),
                        ProfileUrl = SteamIds.ProfileUrl(latest.CommunityId),
                        FirstConnectedAtUtc = group.Min(c => c.FirstSeenAtUtc),
                        LastConnectedAtUtc = latest.LastSeenAtUtc,
                        ConnectionCount = group.Sum(c => c.Count)
                    };
                })
                .OrderByDescending(player => player.LastConnectedAtUtc)
        ];
    }
}
