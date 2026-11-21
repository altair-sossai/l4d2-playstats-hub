using System.Linq.Expressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerDirectoryService(HubDbContext dbContext, IRankingService rankingService) : IPlayerDirectoryService
{
    public async Task<Page<PlayerDirectoryEntry>> SearchAsync(string? term, int page, CancellationToken cancellationToken = default)
    {
        var search = PlayerSearch.Of(term);
        var size = AppConsts.PlayerDirectoryPageSize;
        var profiles = dbContext.PlayerProfiles.AsNoTracking().Where(profile => profile.CommunityId > AppConsts.SteamCommunityIdBase);

        if (search.CommunityId != null)
        {
            var communityId = search.CommunityId.Value;
            var found = await PageAsync(profiles.Where(profile => profile.CommunityId == communityId), 1, size, cancellationToken);

            return found.Total > 0 ? found : new Page<PlayerDirectoryEntry>([new PlayerDirectoryEntry(communityId, false)], 1, size, 1);
        }

        return await PageAsync(search.IsEmpty ? profiles : profiles.Where(Filter(search)), page, size, cancellationToken);
    }

    private Expression<Func<PlayerProfile, bool>> Filter(PlayerSearch search)
    {
        var lower = search.Lower;
        var players = dbContext.MatchPlayers;
        var connections = dbContext.Set<PlayerConnection>();

        Expression<Func<PlayerProfile, bool>> used = profile =>
            players.Any(player => player.CommunityId == profile.CommunityId && player.Name != null && player.Name.ToLower().Contains(lower))
            || connections.Any(connection => connection.CommunityId == profile.CommunityId && connection.Name.ToLower().Contains(lower));

        return search
            .Filter<PlayerProfile>(profile => profile.CommunityId, profile => profile.DisplayName, profile => profile.Name, profile => profile.SteamName)
            .Or(used);
    }

    private async Task<Page<PlayerDirectoryEntry>> PageAsync(IQueryable<PlayerProfile> profiles, int page, int size, CancellationToken cancellationToken)
    {
        var connections = dbContext.Set<PlayerConnection>();

        var found = await profiles
            .Select(profile => new
            {
                profile.CommunityId,
                profile.DisplayName,
                profile.Name,
                profile.LastSeenAtUtc,
                ConnectedAtUtc = connections.Where(connection => connection.CommunityId == profile.CommunityId).Max(connection => (DateTime?)connection.LastSeenAtUtc)
            })
            .Select(profile => new
            {
                profile.CommunityId,
                profile.DisplayName,
                profile.Name,
                LastSeenAtUtc = profile.ConnectedAtUtc != null && profile.ConnectedAtUtc > profile.LastSeenAtUtc ? profile.ConnectedAtUtc.Value : profile.LastSeenAtUtc
            })
            .OrderByDescending(profile => profile.LastSeenAtUtc)
            .ThenBy(profile => profile.DisplayName ?? profile.Name)
            .ThenBy(profile => profile.CommunityId)
            .ToPageAsync(page, size, cancellationToken);

        List<long> ids = [.. found.Items.Select(profile => profile.CommunityId)];
        var used = await UsedNamesAsync(ids, cancellationToken);
        var current = await rankingService.CurrentAsync(cancellationToken);
        var allTime = await rankingService.AllTimeAsync(cancellationToken);
        var positions = current.Where(player => ids.Contains(player.CommunityId)).ToDictionary(player => player.CommunityId, player => player.Position);
        var ranked = allTime.Where(player => ids.Contains(player.CommunityId)).Select(player => player.CommunityId).ToHashSet();

        return found.Map(profile =>
        {
            var entry = new PlayerDirectoryEntry(profile.CommunityId)
            {
                LastSeenAtUtc = profile.LastSeenAtUtc,
                Position = positions.TryGetValue(profile.CommunityId, out var position) ? position : null,
                Ranked = positions.ContainsKey(profile.CommunityId) || ranked.Contains(profile.CommunityId)
            };

            AddName(entry, PlayerDisplayNames.Known(profile.CommunityId, profile.DisplayName));
            AddName(entry, profile.Name);

            foreach (var name in used[profile.CommunityId])
                AddName(entry, name);

            return entry;
        });
    }

    private async Task<ILookup<long, string>> UsedNamesAsync(List<long> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return Array.Empty<string>().ToLookup(_ => 0L);

        var played = await dbContext.MatchPlayers
            .AsNoTracking()
            .Where(player => ids.Contains(player.CommunityId) && player.Name != null && player.Name != string.Empty)
            .GroupBy(player => new { player.CommunityId, player.Name })
            .Select(group => new PlayerNameUsage(group.Key.CommunityId, group.Key.Name!, group.Count(), group.Min(player => player.Id)))
            .ToListAsync(cancellationToken);

        var connected = await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .Where(connection => ids.Contains(connection.CommunityId))
            .Select(connection => new { connection.CommunityId, connection.Name, connection.LastSeenAtUtc })
            .ToListAsync(cancellationToken);

        return ids
            .SelectMany(id => MostUsedNames.Rank(played.Where(usage => usage.CommunityId == id))
                .Concat(connected.Where(connection => connection.CommunityId == id).OrderByDescending(connection => connection.LastSeenAtUtc).Select(connection => connection.Name))
                .Take(AppConsts.PlayerDirectoryNames)
                .Select(name => new { Id = id, Name = name }))
            .ToLookup(entry => entry.Id, entry => entry.Name);
    }

    private static void AddName(PlayerDirectoryEntry entry, string? name)
    {
        var text = name?.Trim();

        if (string.IsNullOrEmpty(text))
            return;

        entry.Name ??= text;

        if (!entry.Names.Contains(text, StringComparer.CurrentCultureIgnoreCase))
            entry.Names.Add(text);
    }
}
