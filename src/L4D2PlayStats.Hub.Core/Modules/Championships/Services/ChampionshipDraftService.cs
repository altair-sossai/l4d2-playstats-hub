using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipDraftService(HubDbContext dbContext, IClock clock, IChampionshipLoadService loader, IChampionshipRandomService random, IChampionshipLogService log, IChampionshipNotificationService notify) : IChampionshipDraftService
{
    public async Task DrawOrderAsync(int championshipId, long? by, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);

        if (!IsOrdered(championship))
            throw new ChampionshipException("Choose the snake or the linear draft in the settings first.");

        if (championship.DraftPicks.Count > 0)
            throw new ChampionshipException("The draft already started. Reset it to draw the order again.");

        if (championship.Teams.Count < 2)
            throw new ChampionshipException("Create at least two teams first.");

        if (championship.Teams.Any(team => team.CaptainCommunityId == null))
            throw new ChampionshipException("Every team needs a captain before the draft.");

        var order = ChampionshipShuffle.Shuffle(championship.Teams, random.Next);

        for (var index = 0; index < order.Count; index++)
            order[index].DraftPosition = index + 1;

        log.Add(championshipId, by, "Drew the order of the draft");
        notify.DraftOrdered(championship);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ChampionshipDraftPick> PickAsync(int championshipId, long communityId, long? by, CancellationToken cancellationToken = default)
    {
        return PickAsync(championshipId, communityId, by, false, cancellationToken);
    }

    public Task<ChampionshipDraftPick> CaptainPickAsync(int championshipId, long communityId, long captain, CancellationToken cancellationToken = default)
    {
        return PickAsync(championshipId, communityId, captain, true, cancellationToken);
    }

    private async Task<ChampionshipDraftPick> PickAsync(int championshipId, long communityId, long? by, bool captain, CancellationToken cancellationToken)
    {
        var championship = await Load(championshipId, cancellationToken);

        if (!IsOrdered(championship))
            throw new ChampionshipException("Choose the snake or the linear draft in the settings first.");

        if (captain && (!championship.IsPublic || championship.IsOver))
            throw new ChampionshipException("The draft is closed.");

        var team = OnTheClock(championship) ?? throw new ChampionshipException("The draft is complete.");

        if (captain && !team.IsLeader(by ?? 0))
            throw new ChampionshipException("It is not the turn of your team.");

        if (!Free(championship).Contains(communityId))
            throw new ChampionshipException("The player is not available in the draft.");

        var pick = Record(championship, team, communityId);

        log.Add(championshipId, by, "Draft pick {0}: {1} chose {2}", pick.Number, team.Name, communityId);
        notify.Picked(championship, team, communityId);
        await dbContext.SaveChangesAsync(cancellationToken);

        return pick;
    }

    public async Task<ChampionshipDraftPick> SpinAsync(int championshipId, long? by, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);

        if (championship.DraftMethod != ChampionshipDraftMethod.Roulette)
            throw new ChampionshipException("Choose the roulette in the settings first.");

        var open = championship.Teams.Where(team => team.Members.Count < championship.RosterSize).ToList();
        var free = Free(championship);

        if (open.Count == 0 || free.Count == 0)
            throw new ChampionshipException("The draft is complete.");

        var fewest = open.Min(team => team.Members.Count);
        var teams = open.Where(team => team.Members.Count == fewest).OrderBy(team => team.Seed).ThenBy(team => team.Id).ToList();
        var team = teams[random.Next(teams.Count)];
        var player = free[random.Next(free.Count)];
        var pick = Record(championship, team, player);

        log.Add(championshipId, by, "Draft pick {0}: {1} chose {2}", pick.Number, team.Name, player);
        notify.MemberAdded(championship, team, player);
        await dbContext.SaveChangesAsync(cancellationToken);

        return pick;
    }

    public async Task<int> DrawPotsAsync(int championshipId, long? by, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);

        if (championship.DraftMethod != ChampionshipDraftMethod.Pots)
            throw new ChampionshipException("Choose the pots in the settings first.");

        if (championship.Teams.Count == 0)
            throw new ChampionshipException("Create at least two teams first.");

        var free = Free(championship).ToHashSet();
        var pots = championship.Participants.Where(participant => participant.Pot > 0 && free.Contains(participant.CommunityId)).GroupBy(participant => participant.Pot).OrderBy(group => group.Key).ToList();

        if (pots.Count == 0)
            throw new ChampionshipException("Put the participants in pots first.");

        var first = pots[0].Key;
        var count = 0;

        foreach (var pot in pots)
        {
            var players = ChampionshipShuffle.Shuffle(pot.Select(participant => participant.CommunityId), random.Next);
            var teams = ChampionshipShuffle.Shuffle(championship.Teams.Where(team => team.Members.Count < championship.RosterSize), random.Next);

            for (var index = 0; index < Math.Min(players.Count, teams.Count); index++)
            {
                Record(championship, teams[index], players[index]);
                notify.MemberAdded(championship, teams[index], players[index]);

                if (pot.Key == first && teams[index].CaptainCommunityId == null)
                    teams[index].CaptainCommunityId = players[index];

                count++;
            }
        }

        log.Add(championshipId, by, "Drew the pots: {0} players placed", count);
        await dbContext.SaveChangesAsync(cancellationToken);

        return count;
    }

    public async Task UndoAsync(int championshipId, long? by, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);
        var last = championship.DraftPicks.MaxBy(pick => pick.Number) ?? throw new ChampionshipException("There is no pick to undo.");
        var team = championship.Teams.FirstOrDefault(item => item.Id == last.TeamId);

        if (team != null && team.CaptainCommunityId != last.CommunityId)
            ChampionshipTeamService.RemoveFromTeam(team, last.CommunityId);

        championship.DraftPicks.Remove(last);
        log.Add(championshipId, by, "Undid the draft pick {0}", last.Number);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(int championshipId, long? by, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);

        foreach (var pick in championship.DraftPicks.ToList())
        {
            var team = championship.Teams.FirstOrDefault(item => item.Id == pick.TeamId);

            if (team != null && (team.CaptainCommunityId != pick.CommunityId || championship.DraftMethod == ChampionshipDraftMethod.Pots))
                ChampionshipTeamService.RemoveFromTeam(team, pick.CommunityId);

            championship.DraftPicks.Remove(pick);
        }

        foreach (var team in championship.Teams)
            team.DraftPosition = null;

        log.Add(championshipId, by, "Reset the draft");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static bool IsOrdered(Championship championship)
    {
        return championship.DraftMethod is ChampionshipDraftMethod.Snake or ChampionshipDraftMethod.Linear;
    }

    public static ChampionshipTeam? OnTheClock(Championship championship)
    {
        var order = championship.Teams.Where(team => team.DraftPosition != null).OrderBy(team => team.DraftPosition).ToList();

        if (order.Count == 0 || order.Count != championship.Teams.Count)
            return null;

        if (Free(championship).Count == 0 || order.All(team => team.Members.Count >= championship.RosterSize))
            return null;

        var snake = championship.DraftMethod == ChampionshipDraftMethod.Snake;

        for (var index = championship.DraftPicks.Count; index < championship.DraftPicks.Count + order.Count * AppConsts.ChampionshipMaxRoster; index++)
        {
            var team = order[DraftOrder.Position(index, order.Count, snake)];

            if (team.Members.Count < championship.RosterSize)
                return team;
        }

        return null;
    }

    public static List<long> Free(Championship championship)
    {
        var taken = championship.Teams.SelectMany(team => team.Members).Select(member => member.CommunityId).ToHashSet();

        return championship.Participants.Where(participant => !taken.Contains(participant.CommunityId)).OrderBy(participant => participant.AddedAtUtc).ThenBy(participant => participant.Id).Select(participant => participant.CommunityId).ToList();
    }

    private ChampionshipDraftPick Record(Championship championship, ChampionshipTeam team, long communityId)
    {
        ChampionshipTeamService.AddMember(championship, team, communityId, clock.UtcNow);

        var pick = new ChampionshipDraftPick
        {
            Number = championship.DraftPicks.Count == 0 ? 1 : championship.DraftPicks.Max(item => item.Number) + 1,
            TeamId = team.Id,
            CommunityId = communityId,
            PickedAtUtc = clock.UtcNow
        };

        championship.DraftPicks.Add(pick);

        return pick;
    }

    private async Task<Championship> Load(int championshipId, CancellationToken cancellationToken)
    {
        return await loader.LoadAsync(championshipId, true, cancellationToken) ?? throw new ChampionshipException("The championship was not found.");
    }
}
