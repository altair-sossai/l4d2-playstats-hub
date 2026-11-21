using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using System.Linq.Expressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Matches;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityPlayerService(HubDbContext dbContext, IForcedNameStore forcedNames) : ICommunityPlayerService
{
    private static readonly Expression<Func<MatchPlayer, bool>> TookPart = p => p.TeamA || p.TeamB || p.Win || p.Loss || p.Draws > 0 || p.RageQuits > 0;

    public Task<int> MatchesAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return Played().CountAsync(p => p.CommunityId == communityId, cancellationToken);
    }

    public async Task<HashSet<long>> CandidatesAsync(int minimumMatches, DateTime? playedSince = null, CancellationToken cancellationToken = default)
    {
        var players = Played().Where(p => p.CommunityId > AppConsts.SteamCommunityIdBase);

        if (playedSince != null)
        {
            var since = playedSince.Value;
            var recent = Played().Where(p => p.Match!.LastFileDate >= since).Select(p => p.CommunityId);

            players = players.Where(p => recent.Contains(p.CommunityId));
        }

        var candidates = await players
            .GroupBy(p => p.CommunityId)
            .Where(group => group.Count() >= minimumMatches)
            .Select(group => group.Key)
            .ToListAsync(cancellationToken);

        return [.. candidates];
    }

    public async Task<HashSet<long>> FamiliarAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var matchIds = Played().Where(p => p.CommunityId == communityId).Select(p => p.MatchId);

        var familiar = await Played()
            .Where(p => p.CommunityId != communityId && matchIds.Contains(p.MatchId))
            .Select(p => p.CommunityId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return [.. familiar];
    }

    public async Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        var ids = communityIds.Distinct().ToList();

        var names = await dbContext.PlayerProfiles
            .AsNoTracking()
            .Where(p => ids.Contains(p.CommunityId) && p.Name != null && p.Name != string.Empty)
            .ToDictionaryAsync(p => p.CommunityId, p => p.Name!, cancellationToken);

        return ids.ToDictionary(id => id, id => forcedNames.Find(id) ?? names.GetValueOrDefault(id) ?? id.ToString());
    }

    public async Task<Dictionary<long, List<string>>> MostUsedNamesAsync(IReadOnlyCollection<long> communityIds, int count, CancellationToken cancellationToken = default)
    {
        var ids = communityIds.Distinct().ToList();

        var used = await Played()
            .Where(p => ids.Contains(p.CommunityId) && p.Name != null && p.Name != string.Empty)
            .GroupBy(p => new { p.CommunityId, p.Name })
            .Select(group => new { group.Key.CommunityId, Name = group.Key.Name!, Times = group.Count() })
            .ToListAsync(cancellationToken);

        return ids.ToDictionary(id => id, id => used
            .Where(name => name.CommunityId == id)
            .GroupBy(name => name.Name.Trim(), StringComparer.CurrentCultureIgnoreCase)
            .Select(group => new { Name = group.OrderByDescending(name => name.Times).First().Name.Trim(), Times = group.Sum(name => name.Times) })
            .OrderByDescending(name => name.Times)
            .ThenBy(name => name.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(count)
            .Select(name => name.Name)
            .ToList());
    }

    private IQueryable<MatchPlayer> Played()
    {
        return dbContext.MatchPlayers.AsNoTracking().Where(TookPart);
    }
}
