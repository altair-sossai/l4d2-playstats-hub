using L4D2PlayStats.Hub.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Support.Skins;

public static class StarsOracle
{
    public static async Task<Dictionary<long, int>> CountAsync(IServiceProvider services)
    {
        var rows = await services.GetRequiredService<HubDbContext>().MatchPlayers
            .AsNoTracking()
            .Where(p => p.Match!.CountsForRanking && (p.TeamA || p.TeamB))
            .Select(p => new { p.CommunityId, p.TeamA, p.TeamB, p.MvpSiDamage, p.MvpCommon })
            .ToListAsync();

        return rows
            .GroupBy(row => row.CommunityId)
            .ToDictionary(group => group.Key, group => group.Sum(row => (row.MvpSiDamage + row.MvpCommon) * ((row.TeamA ? 1 : 0) + (row.TeamB ? 1 : 0))));
    }
}
