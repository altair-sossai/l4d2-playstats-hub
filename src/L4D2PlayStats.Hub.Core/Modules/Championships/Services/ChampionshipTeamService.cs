using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipTeamService(HubDbContext dbContext, IClock clock, IChampionshipLoadService loader, IChampionshipLogService log) : IChampionshipTeamService
{
    public async Task<int> AddParticipantsAsync(int championshipId, IReadOnlyCollection<long> communityIds, long? addedBy, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);
        var existing = championship.Participants.Select(participant => participant.CommunityId).ToHashSet();
        var added = 0;

        foreach (var communityId in communityIds.Where(id => id > 0).Distinct())
        {
            if (existing.Contains(communityId))
                continue;

            if (championship.MaxParticipants != null && championship.Participants.Count >= championship.MaxParticipants)
                throw new ChampionshipException("The championship already has the maximum number of participants.");

            championship.Participants.Add(new ChampionshipParticipant { CommunityId = communityId, AddedAtUtc = clock.UtcNow });
            existing.Add(communityId);
            added++;
        }

        if (added > 0)
            log.Add(championshipId, addedBy, "Added {0} participants", added);

        await dbContext.SaveChangesAsync(cancellationToken);

        return added;
    }

    public async Task RemoveParticipantAsync(int championshipId, long communityId, long? removedBy, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);
        var participant = championship.Participants.FirstOrDefault(item => item.CommunityId == communityId);

        if (participant == null)
            return;

        foreach (var team in championship.Teams)
            RemoveFromTeam(team, communityId);

        foreach (var pick in championship.DraftPicks.Where(pick => pick.CommunityId == communityId).ToList())
            championship.DraftPicks.Remove(pick);

        championship.Participants.Remove(participant);
        log.Add(championshipId, removedBy, "Removed the participant {0}", communityId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPotAsync(int championshipId, long communityId, int pot, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);
        var participant = championship.Participants.FirstOrDefault(item => item.CommunityId == communityId) ?? throw new ChampionshipException("The player is not a participant of this championship.");

        participant.Pot = Math.Clamp(pot, 0, AppConsts.ChampionshipMaxTeams);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChampionshipTeam> CreateTeamAsync(int championshipId, string name, string? tag, long? createdBy, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);
        var team = NewTeam(championship, name, tag);

        log.Add(championshipId, createdBy, "Created the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        return team;
    }

    public async Task<int> CreateTeamsAsync(int championshipId, int count, string prefix, long? createdBy, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);
        var total = Math.Clamp(count, 1, AppConsts.ChampionshipMaxTeams - championship.Teams.Count);
        var names = championship.Teams.Select(team => team.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var created = 0;

        for (var index = 0; created < total && index < AppConsts.ChampionshipMaxTeams * 2; index++)
        {
            var name = $"{prefix.Trim()} {Label(index)}".Trim();

            if (!names.Add(name))
                continue;

            NewTeam(championship, name, null);
            created++;
        }

        log.Add(championshipId, createdBy, "Created {0} teams", created);
        await dbContext.SaveChangesAsync(cancellationToken);

        return created;
    }

    public async Task UpdateTeamAsync(int teamId, string name, string? tag, int seed, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadByTeamAsync(teamId, cancellationToken) ?? throw TeamNotFound();
        var team = championship.Teams.First(item => item.Id == teamId);

        team.Name = TeamName(championship, name, teamId);
        team.Tag = Tag(tag);
        team.Seed = Math.Clamp(seed, 1, AppConsts.ChampionshipMaxTeams);
        log.Add(championship.Id, updatedBy, "Saved the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTeamAsync(int teamId, long? deletedBy, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadByTeamAsync(teamId, cancellationToken) ?? throw TeamNotFound();
        var team = championship.Teams.First(item => item.Id == teamId);

        if (championship.Stages.Any(stage => stage.Groups.Any(group => group.Teams.Any(entry => entry.TeamId == teamId)) || stage.Series.Any(series => series.Has(teamId))))
            throw new ChampionshipException("The team is already in a stage. Reset the stage before deleting it.");

        if (championship.Stages.Any(stage => stage.Penalties.Any(penalty => penalty.TeamId == teamId)))
            throw new ChampionshipException("The team is already in a stage. Reset the stage before deleting it.");

        foreach (var pick in championship.DraftPicks.Where(pick => pick.TeamId == teamId).ToList())
            championship.DraftPicks.Remove(pick);

        championship.Teams.Remove(team);
        log.Add(championship.Id, deletedBy, "Deleted the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddMemberAsync(int teamId, long communityId, long? addedBy, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadByTeamAsync(teamId, cancellationToken) ?? throw TeamNotFound();
        var team = championship.Teams.First(item => item.Id == teamId);

        AddMember(championship, team, communityId, clock.UtcNow);
        log.Add(championship.Id, addedBy, "Added {0} to the team {1}", communityId, team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(int teamId, long communityId, long? removedBy, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadByTeamAsync(teamId, cancellationToken) ?? throw TeamNotFound();
        var team = championship.Teams.First(item => item.Id == teamId);

        RemoveFromTeam(team, communityId);

        foreach (var pick in championship.DraftPicks.Where(pick => pick.TeamId == teamId && pick.CommunityId == communityId).ToList())
            championship.DraftPicks.Remove(pick);

        log.Add(championship.Id, removedBy, "Removed {0} from the team {1}", communityId, team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetLeadersAsync(int teamId, long? captain, long? viceCaptain, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadByTeamAsync(teamId, cancellationToken) ?? throw TeamNotFound();
        var team = championship.Teams.First(item => item.Id == teamId);

        if (captain != null && captain == viceCaptain)
            throw new ChampionshipException("The captain and the vice-captain must be different players.");

        foreach (var leader in new[] { captain, viceCaptain }.Where(id => id != null).Select(id => id!.Value))
            if (team.Members.All(member => member.CommunityId != leader))
                AddMember(championship, team, leader, clock.UtcNow);

        team.CaptainCommunityId = captain;
        team.ViceCaptainCommunityId = viceCaptain;
        log.Add(championship.Id, updatedBy, "Saved the leaders of the team {0}", team.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static void AddMember(Championship championship, ChampionshipTeam team, long communityId, DateTime now)
    {
        if (team.Members.Any(member => member.CommunityId == communityId))
            return;

        if (championship.Teams.Any(other => other.Id != team.Id && other.Members.Any(member => member.CommunityId == communityId)))
            throw new ChampionshipException("The player is already in another team.");

        if (team.Members.Count >= championship.RosterSize)
            throw new ChampionshipException("The team is already full.");

        if (championship.Participants.All(participant => participant.CommunityId != communityId))
        {
            if (championship.MaxParticipants != null && championship.Participants.Count >= championship.MaxParticipants)
                throw new ChampionshipException("The championship already has the maximum number of participants.");

            championship.Participants.Add(new ChampionshipParticipant { CommunityId = communityId, AddedAtUtc = now });
        }

        team.Members.Add(new ChampionshipTeamMember { CommunityId = communityId, JoinedAtUtc = now });
    }

    public static void RemoveFromTeam(ChampionshipTeam team, long communityId)
    {
        var member = team.Members.FirstOrDefault(item => item.CommunityId == communityId);

        if (member != null)
            team.Members.Remove(member);

        if (team.CaptainCommunityId == communityId)
            team.CaptainCommunityId = null;

        if (team.ViceCaptainCommunityId == communityId)
            team.ViceCaptainCommunityId = null;
    }

    private static ChampionshipTeam NewTeam(Championship championship, string name, string? tag)
    {
        if (championship.Teams.Count >= AppConsts.ChampionshipMaxTeams)
            throw new ChampionshipException("The championship already has the maximum number of teams.");

        var team = new ChampionshipTeam
        {
            Name = TeamName(championship, name, null),
            Tag = Tag(tag),
            Seed = championship.Teams.Count == 0 ? 1 : championship.Teams.Max(item => item.Seed) + 1
        };

        championship.Teams.Add(team);

        return team;
    }

    private static string TeamName(Championship championship, string? name, int? teamId)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new ChampionshipException("Enter the name of the team.");

        if (trimmed.Length > AppConsts.ChampionshipTeamNameMaxLength)
            trimmed = trimmed[..AppConsts.ChampionshipTeamNameMaxLength];

        if (championship.Teams.Any(team => team.Id != teamId && string.Equals(team.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
            throw new ChampionshipException("There is already a team with this name.");

        return trimmed;
    }

    private static string? Tag(string? tag)
    {
        var trimmed = tag?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return null;

        return trimmed.Length > AppConsts.ChampionshipTeamTagMaxLength ? trimmed[..AppConsts.ChampionshipTeamTagMaxLength] : trimmed;
    }

    private static string Label(int index)
    {
        return index < AppConsts.ChampionshipGroupNames.Length ? AppConsts.ChampionshipGroupNames[index].ToString() : (index + 1).ToString();
    }

    private async Task<Championship> Load(int championshipId, CancellationToken cancellationToken)
    {
        return await loader.LoadAsync(championshipId, true, cancellationToken) ?? throw new ChampionshipException("The championship was not found.");
    }

    private static ChampionshipException TeamNotFound()
    {
        return new ChampionshipException("The team was not found.");
    }
}
