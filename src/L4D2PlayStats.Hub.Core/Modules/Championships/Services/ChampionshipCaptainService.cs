using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipCaptainService(HubDbContext dbContext, IClock clock, IChampionshipLoadService loader, IChampionshipLogService log) : IChampionshipCaptainService
{
    public async Task<List<ChampionshipCaptainTeam>> TeamsAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.ChampionshipTeams.AsNoTracking()
            .Where(team => team.CaptainCommunityId == communityId || team.ViceCaptainCommunityId == communityId)
            .Join(dbContext.Championships, team => team.ChampionshipId, championship => championship.Id, (team, championship) => new { team.Id, team.ChampionshipId, championship.Status })
            .Where(item => item.Status != ChampionshipStatus.Draft && item.Status != ChampionshipStatus.Finished && item.Status != ChampionshipStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var teams = new List<ChampionshipCaptainTeam>();

        foreach (var row in rows)
        {
            var championship = await loader.LoadAsync(row.ChampionshipId, false, cancellationToken);
            var team = championship?.Teams.FirstOrDefault(item => item.Id == row.Id);

            if (championship == null || team == null)
                continue;

            teams.Add(new ChampionshipCaptainTeam(championship.Id, championship.Slug, championship.Name, team.Id, team.Name, team.CaptainCommunityId == communityId, CaptainTasks.Of(championship, team, clock.UtcNow)));
        }

        return teams.OrderByDescending(team => team.Tasks.Count).ThenBy(team => team.Championship).ToList();
    }

    public async Task SetViceAsync(int teamId, long captain, long? viceCaptain, CancellationToken cancellationToken = default)
    {
        var (championship, team) = await loader.TeamAsync(teamId, cancellationToken);

        if (team.CaptainCommunityId != captain)
            throw new ChampionshipException("Only the captain chooses the vice-captain.");

        if (championship.IsOver)
            throw new ChampionshipException("The championship is over.");

        if (viceCaptain != null && (viceCaptain == captain || team.Members.All(member => member.CommunityId != viceCaptain)))
            throw new ChampionshipException("Choose a player of your team.");

        team.ViceCaptainCommunityId = viceCaptain;
        log.Add(championship.Id, captain, "The captain chose the vice-captain of the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
