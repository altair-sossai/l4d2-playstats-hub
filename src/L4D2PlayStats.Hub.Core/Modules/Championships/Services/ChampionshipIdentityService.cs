using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipIdentityService(HubDbContext dbContext, IClock clock, IChampionshipLoadService loader, IChampionshipLogService log, IChampionshipNotificationService notify) : IChampionshipIdentityService
{
    public async Task<List<string>> ProposeAsync(int teamId, long leader, string? name, string? tag, string? flag, CancellationToken cancellationToken = default)
    {
        var (championship, team) = await Leader(teamId, leader, cancellationToken);
        var removed = new List<string>();
        var proposedName = ChampionshipTeamService.TeamName(championship, string.IsNullOrWhiteSpace(name) ? team.Name : name, team.Id);
        var proposedTag = ChampionshipTeamService.Tag(tag);
        var nameChanged = proposedName != team.Name || proposedTag != team.Tag;
        var waiting = team.PendingAtUtc != null;

        if (!nameChanged && flag == null)
            throw new ChampionshipException("Change the name, the tag or the flag first.");

        if (nameChanged)
        {
            team.PendingName = proposedName;
            team.PendingTag = proposedTag;
        }

        if (flag != null)
        {
            if (team.PendingFlag != null)
                removed.Add(team.PendingFlag);

            team.PendingFlag = flag;
        }

        team.PendingAtUtc = clock.UtcNow;
        team.PendingByCommunityId = leader;
        team.RejectionReason = null;
        log.Add(championship.Id, leader, "Sent a new name, tag or flag of the team {0} for approval", team.Name);
        if (!waiting)
            notify.IdentitySent(championship, team);

        await dbContext.SaveChangesAsync(cancellationToken);

        return removed;
    }

    public async Task<List<string>> CancelAsync(int teamId, long leader, CancellationToken cancellationToken = default)
    {
        var (championship, team) = await Leader(teamId, leader, cancellationToken);
        var removed = Clear(team);

        log.Add(championship.Id, leader, "Withdrew the new name, tag or flag of the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        return removed;
    }

    public async Task<List<string>> RemoveFlagAsync(int teamId, long? by, bool leader, CancellationToken cancellationToken = default)
    {
        var (championship, team) = leader ? await Leader(teamId, by ?? 0, cancellationToken) : await Team(teamId, cancellationToken);
        var removed = new List<string>();

        if (team.Flag == null)
            return removed;

        removed.Add(team.Flag);
        team.Flag = null;
        log.Add(championship.Id, by, "Removed the flag of the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        return removed;
    }

    public async Task<List<string>> SetFlagAsync(int teamId, string flag, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, team) = await Team(teamId, cancellationToken);
        var removed = new List<string>();

        if (team.Flag != null)
            removed.Add(team.Flag);

        team.Flag = flag;
        log.Add(championship.Id, by, "Changed the flag of the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        return removed;
    }

    public async Task<List<string>> ApproveAsync(int teamId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, team) = await Team(teamId, cancellationToken);
        var removed = new List<string>();

        if (!team.HasPendingIdentity)
            throw new ChampionshipException("The team has nothing waiting for approval.");

        if (team.PendingName != null)
        {
            team.Name = ChampionshipTeamService.TeamName(championship, team.PendingName, team.Id);
            team.Tag = team.PendingTag;
        }

        if (team.PendingFlag != null)
        {
            if (team.Flag != null)
                removed.Add(team.Flag);

            team.Flag = team.PendingFlag;
        }

        team.PendingName = null;
        team.PendingTag = null;
        team.PendingFlag = null;
        team.PendingAtUtc = null;
        team.PendingByCommunityId = null;
        team.RejectionReason = null;
        log.Add(championship.Id, by, "Approved the new name, tag or flag of the team {0}", team.Name);
        notify.IdentityAnswered(championship, team, true);
        await dbContext.SaveChangesAsync(cancellationToken);

        return removed;
    }

    public async Task<List<string>> RejectAsync(int teamId, string? reason, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, team) = await Team(teamId, cancellationToken);

        if (!team.HasPendingIdentity)
            throw new ChampionshipException("The team has nothing waiting for approval.");

        var removed = Clear(team);
        team.RejectionReason = Text.Clip(reason, AppConsts.ChampionshipReasonMaxLength) ?? string.Empty;
        log.Add(championship.Id, by, "Refused the new name, tag or flag of the team {0}", team.Name);
        notify.IdentityAnswered(championship, team, false);
        await dbContext.SaveChangesAsync(cancellationToken);

        return removed;
    }

    public async Task<List<string>> FilesAsync(int championshipId, int? teamId = null, CancellationToken cancellationToken = default)
    {
        var teams = await dbContext.ChampionshipTeams
            .Where(team => team.ChampionshipId == championshipId && (teamId == null || team.Id == teamId))
            .Select(team => new { team.Flag, team.PendingFlag })
            .ToListAsync(cancellationToken);

        return teams.SelectMany(team => new[] { team.Flag, team.PendingFlag }).OfType<string>().ToList();
    }

    private static List<string> Clear(ChampionshipTeam team)
    {
        var removed = team.PendingFlag == null ? new List<string>() : [team.PendingFlag];

        team.PendingName = null;
        team.PendingTag = null;
        team.PendingFlag = null;
        team.PendingAtUtc = null;
        team.PendingByCommunityId = null;

        return removed;
    }

    private async Task<(Championship Championship, ChampionshipTeam Team)> Leader(int teamId, long leader, CancellationToken cancellationToken)
    {
        var (championship, team) = await Team(teamId, cancellationToken);

        if (!team.IsLeader(leader))
            throw new ChampionshipException("Only the captain and the vice-captain manage the team.");

        if (!championship.IsPublic || championship.IsOver)
            throw new ChampionshipException("The championship is over.");

        return (championship, team);
    }

    private async Task<(Championship Championship, ChampionshipTeam Team)> Team(int teamId, CancellationToken cancellationToken)
    {
        return await loader.TeamAsync(teamId, cancellationToken);
    }
}
