using System.Globalization;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Models;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Services;

public class PlayerDirectoryService(HubDbContext dbContext, IRankingService rankingService) : IPlayerDirectoryService
{
    public async Task<PlayerDirectoryPage> SearchAsync(string? term, int page, CancellationToken cancellationToken = default)
    {
        var entries = await EntriesAsync(cancellationToken);
        var matches = Matches(entries, term?.Trim());
        var size = AppConsts.PlayerDirectoryPageSize;
        var number = Math.Clamp(page, 1, matches.Count / size + 1);

        var players = matches
            .OrderByDescending(entry => entry.LastSeenAtUtc)
            .ThenBy(entry => entry.Position ?? int.MaxValue)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(entry => entry.CommunityId)
            .Skip((number - 1) * size)
            .Take(size)
            .ToList();

        return new PlayerDirectoryPage(players, matches.Count, number, number * size < matches.Count);
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

    private static List<PlayerDirectoryEntry> Matches(List<PlayerDirectoryEntry> entries, string? term)
    {
        if (string.IsNullOrEmpty(term))
            return entries;

        var communityId = SteamIdParser.Parse(term);

        if (communityId == null)
            return [.. entries.Where(entry => Contains(entry, term))];

        var found = entries.Where(entry => entry.CommunityId == communityId).ToList();

        return found.Count > 0 ? found : [new PlayerDirectoryEntry(communityId.Value, false)];
    }

    private static bool Contains(PlayerDirectoryEntry entry, string term)
    {
        return entry.Names.Any(name => name.Contains(term, StringComparison.CurrentCultureIgnoreCase))
               || entry.CommunityId.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.Ordinal)
               || SteamIds.SteamId(entry.CommunityId).Contains(term, StringComparison.OrdinalIgnoreCase)
               || SteamIds.Steam3(entry.CommunityId).Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}