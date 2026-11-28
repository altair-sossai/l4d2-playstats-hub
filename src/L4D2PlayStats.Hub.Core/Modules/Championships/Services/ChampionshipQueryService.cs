using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipQueryService(HubDbContext dbContext) : IChampionshipQueryService
{
    public async Task<Dictionary<long, PlayerProfile>> ProfilesAsync(Championship championship, CancellationToken cancellationToken = default)
    {
        var ids = championship.Participants.Select(participant => participant.CommunityId)
            .Concat(championship.Teams.SelectMany(team => team.Members).Select(member => member.CommunityId))
            .Distinct()
            .ToList();

        return await dbContext.PlayerProfiles.AsNoTracking().Where(profile => ids.Contains(profile.CommunityId)).ToDictionaryAsync(profile => profile.CommunityId, cancellationToken);
    }

    public async Task<Dictionary<long, string>> NamesAsync(IReadOnlyCollection<long> communityIds, CancellationToken cancellationToken = default)
    {
        var ids = communityIds.Distinct().ToList();
        var names = await dbContext.Administrators.AsNoTracking().Where(administrator => ids.Contains(administrator.CommunityId) && administrator.Name != null).ToDictionaryAsync(administrator => administrator.CommunityId, administrator => administrator.Name!, cancellationToken);
        var profiles = await dbContext.PlayerProfiles.AsNoTracking().Where(profile => ids.Contains(profile.CommunityId)).Select(profile => new { profile.CommunityId, Name = profile.DisplayName ?? profile.Name }).ToListAsync(cancellationToken);

        foreach (var profile in profiles.Where(profile => profile.Name != null))
            names[profile.CommunityId] = profile.Name!;

        return names;
    }

    public async Task<List<ChampionshipPlayerStatistics>> StatisticsAsync(Championship championship, CancellationToken cancellationToken = default)
    {
        var matchIds = championship.Stages.SelectMany(stage => stage.Series).SelectMany(series => series.Games).Where(game => game.IsPlayed && game.MatchId != null).Select(game => game.MatchId!.Value).Distinct().ToList();
        var teams = championship.Teams.SelectMany(team => team.Members.Select(member => (member.CommunityId, TeamId: team.Id))).ToDictionary(pair => pair.CommunityId, pair => pair.TeamId);

        if (matchIds.Count == 0 || teams.Count == 0)
            return [];

        var ids = teams.Keys.ToList();
        var rows = await dbContext.MatchPlayers.AsNoTracking().Where(player => matchIds.Contains(player.MatchId) && ids.Contains(player.CommunityId) && (player.TeamA || player.TeamB)).ToListAsync(cancellationToken);
        var result = new Dictionary<long, ChampionshipPlayerStatistics>();

        foreach (var row in rows)
        {
            if (!result.TryGetValue(row.CommunityId, out var statistics))
            {
                statistics = new ChampionshipPlayerStatistics { CommunityId = row.CommunityId, TeamId = teams[row.CommunityId], Name = row.Name };
                result[row.CommunityId] = statistics;
            }

            statistics.Games++;

            if (row.Win)
                statistics.Wins++;

            statistics.Totals.Add(row);
        }

        return result.Values.OrderByDescending(statistics => statistics.Totals.MvpSiDamage + statistics.Totals.MvpCommon).ThenByDescending(statistics => statistics.Totals.SiDamage).ToList();
    }

    public async Task<ChampionshipMatchLink?> MatchLinkAsync(int matchId, CancellationToken cancellationToken = default)
    {
        var link = await (
                from game in dbContext.ChampionshipGames.AsNoTracking()
                join series in dbContext.ChampionshipSeries on game.SeriesId equals series.Id
                join stage in dbContext.ChampionshipStages on series.StageId equals stage.Id
                join championship in dbContext.Championships on stage.ChampionshipId equals championship.Id
                where game.MatchId == matchId && championship.Status != ChampionshipStatus.Draft
                select new { championship.Name, championship.Slug, Stage = stage.Name, SeriesId = series.Id, game.Number })
            .FirstOrDefaultAsync(cancellationToken);

        return link == null ? null : new ChampionshipMatchLink(link.Name, link.Slug, link.Stage, link.SeriesId, link.Number);
    }

    public Task<ChampionshipTeamFlag?> FlagAsync(int teamId, CancellationToken cancellationToken = default)
    {
        return dbContext.ChampionshipTeams.AsNoTracking()
            .Where(team => team.Id == teamId)
            .Join(dbContext.Championships, team => team.ChampionshipId, championship => championship.Id, (team, championship) => new ChampionshipTeamFlag(team.Flag, team.PendingFlag, championship.Status, team.CaptainCommunityId, team.ViceCaptainCommunityId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
