using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerDirectoryService(HubDbContext dbContext, IRankingService rankingService, IForcedNameStore forcedNames) : IPlayerDirectoryService
{
    public async Task<Page<PlayerDirectoryEntry>> SearchAsync(string? term, int page, CancellationToken cancellationToken = default)
    {
        var entries = await EntriesAsync(cancellationToken);
        var matches = Matches(entries, PlayerSearch.Of(term));
        var size = AppConsts.PlayerDirectoryPageSize;
        var number = Paging.Clamp(page, matches.Count, size);

        var players = matches
            .OrderByDescending(entry => entry.LastSeenAtUtc)
            .ThenBy(entry => entry.Position ?? int.MaxValue)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(entry => entry.CommunityId)
            .Skip(Paging.Skip(number, size))
            .Take(size)
            .ToList();

        return new Page<PlayerDirectoryEntry>(players, number, size, matches.Count);
    }

    private async Task<List<PlayerDirectoryEntry>> EntriesAsync(CancellationToken cancellationToken)
    {
        var current = await rankingService.CurrentAsync(cancellationToken);
        var allTime = await rankingService.AllTimeAsync(cancellationToken);

        var profiles = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Select(profile => new { profile.CommunityId, profile.Name, profile.LastSeenAtUtc })
            .ToListAsync(cancellationToken);

        var connections = await dbContext.Set<PlayerConnection>()
            .AsNoTracking()
            .GroupBy(connection => new { connection.CommunityId, connection.Name })
            .Select(group => new { group.Key.CommunityId, group.Key.Name, LastSeenAtUtc = group.Max(connection => connection.LastSeenAtUtc) })
            .ToListAsync(cancellationToken);

        var entries = new Dictionary<long, PlayerDirectoryEntry>();

        foreach (var (communityId, name) in forcedNames.All.Where(entry => SteamIds.IsValid(entry.Key)))
            AddName(Entry(entries, communityId), name);

        foreach (var player in current.Where(player => SteamIds.IsValid(player.CommunityId)))
        {
            var entry = Entry(entries, player.CommunityId);

            entry.Position = player.Position;
            AddRanked(entry, player);
        }

        foreach (var player in allTime.Where(player => SteamIds.IsValid(player.CommunityId)))
            AddRanked(Entry(entries, player.CommunityId), player);

        foreach (var profile in profiles.Where(profile => SteamIds.IsValid(profile.CommunityId)))
            AddSeen(Entry(entries, profile.CommunityId), profile.Name, profile.LastSeenAtUtc);

        foreach (var connection in connections.Where(connection => SteamIds.IsValid(connection.CommunityId)).OrderByDescending(connection => connection.LastSeenAtUtc))
            AddSeen(Entry(entries, connection.CommunityId), connection.Name, connection.LastSeenAtUtc);

        return [.. entries.Values];
    }

    private static PlayerDirectoryEntry Entry(Dictionary<long, PlayerDirectoryEntry> entries, long communityId)
    {
        if (entries.TryGetValue(communityId, out var entry))
            return entry;

        entry = new PlayerDirectoryEntry(communityId);
        entries[communityId] = entry;

        return entry;
    }

    private static void AddRanked(PlayerDirectoryEntry entry, RankingPlayer player)
    {
        entry.Ranked = true;
        AddName(entry, player.PreviousNames.FirstOrDefault()?.Name ?? player.Name);

        foreach (var previous in player.PreviousNames)
            AddName(entry, previous.Name);
    }

    private static void AddSeen(PlayerDirectoryEntry entry, string? name, DateTime lastSeenAtUtc)
    {
        if (entry.LastSeenAtUtc == null || lastSeenAtUtc > entry.LastSeenAtUtc)
            entry.LastSeenAtUtc = lastSeenAtUtc;

        AddName(entry, name);
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

    private static List<PlayerDirectoryEntry> Matches(List<PlayerDirectoryEntry> entries, PlayerSearch search)
    {
        if (search.IsEmpty)
            return entries;

        if (search.CommunityId == null)
            return [.. entries.Where(entry => search.Matches(entry.CommunityId, entry.Names))];

        var found = entries.Where(entry => entry.CommunityId == search.CommunityId).ToList();

        return found.Count > 0 ? found : [new PlayerDirectoryEntry(search.CommunityId.Value, false)];
    }
}
