using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipJoinService(HubDbContext dbContext, IClock clock, IChampionshipLogService log, IChampionshipNotificationService notify) : IChampionshipJoinService
{
    public bool IsOpen(Championship championship)
    {
        return championship.JoinRequests && championship.Status == ChampionshipStatus.Participants && !IsFull(championship);
    }

    public Task<ChampionshipJoinRequest?> FindAsync(int championshipId, long communityId, CancellationToken cancellationToken = default)
    {
        return dbContext.ChampionshipJoinRequests.AsNoTracking().FirstOrDefaultAsync(request => request.ChampionshipId == championshipId && request.CommunityId == communityId, cancellationToken);
    }

    public async Task<List<ChampionshipJoinRequest>> ListAsync(int championshipId, CancellationToken cancellationToken = default)
    {
        var requests = await dbContext.ChampionshipJoinRequests.AsNoTracking()
            .Where(request => request.ChampionshipId == championshipId && request.Status != ChampionshipJoinRequestStatus.Approved)
            .ToListAsync(cancellationToken);

        return requests.OrderBy(request => request.Status).ThenBy(request => request.CreatedAtUtc).ThenBy(request => request.Id).ToList();
    }

    public async Task<ChampionshipJoinRequest> AskAsync(int championshipId, long communityId, string? message, CancellationToken cancellationToken = default)
    {
        var championship = await Load(championshipId, cancellationToken);

        if (championship.Participants.Any(participant => participant.CommunityId == communityId))
            throw new ChampionshipException("You are already a participant of this championship.");

        if (!championship.JoinRequests || championship.Status != ChampionshipStatus.Participants)
            throw new ChampionshipException("This championship does not take requests to join.");

        if (IsFull(championship))
            throw new ChampionshipException("There are no places left in this championship.");

        var request = await dbContext.ChampionshipJoinRequests.FirstOrDefaultAsync(item => item.ChampionshipId == championshipId && item.CommunityId == communityId, cancellationToken);

        switch (request?.Status)
        {
            case ChampionshipJoinRequestStatus.Pending:
                throw new ChampionshipException("You already asked to join this championship.");
            case ChampionshipJoinRequestStatus.Refused:
                throw new ChampionshipException("The administration refused your request to join this championship.");
        }

        if (request == null)
        {
            request = new ChampionshipJoinRequest { ChampionshipId = championshipId, CommunityId = communityId };
            dbContext.ChampionshipJoinRequests.Add(request);
        }

        request.Message = Message(message);
        request.Status = ChampionshipJoinRequestStatus.Pending;
        request.CreatedAtUtc = clock.UtcNow;
        request.AnsweredAtUtc = null;
        request.AnsweredBy = null;

        log.Add(championshipId, communityId, "{0} asked to join", communityId);
        notify.JoinAsked(championship, communityId);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request;
    }

    public async Task CancelAsync(int championshipId, long communityId, CancellationToken cancellationToken = default)
    {
        var request = await dbContext.ChampionshipJoinRequests.FirstOrDefaultAsync(item => item.ChampionshipId == championshipId && item.CommunityId == communityId, cancellationToken);

        if (request is not { Status: ChampionshipJoinRequestStatus.Pending })
            throw new ChampionshipException("There is no request to cancel.");

        dbContext.ChampionshipJoinRequests.Remove(request);
        log.Add(championshipId, communityId, "{0} cancelled the request to join", communityId);
        notify.JoinCancelled(await Load(championshipId, cancellationToken), communityId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(int requestId, long? answeredBy, CancellationToken cancellationToken = default)
    {
        var request = await Request(requestId, cancellationToken);

        if (request.Status == ChampionshipJoinRequestStatus.Approved)
            throw new ChampionshipException("The request was already answered.");

        var championship = await Load(request.ChampionshipId, cancellationToken);

        if (championship.Participants.All(participant => participant.CommunityId != request.CommunityId))
        {
            if (IsFull(championship))
                throw new ChampionshipException("The championship already has the maximum number of participants.");

            championship.Participants.Add(new ChampionshipParticipant { CommunityId = request.CommunityId, AddedAtUtc = clock.UtcNow });
        }

        Answer(request, ChampionshipJoinRequestStatus.Approved, answeredBy);
        log.Add(request.ChampionshipId, answeredBy, "Approved the request of {0} to join", request.CommunityId);
        notify.JoinAnswered(championship, request.CommunityId, true);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RefuseAsync(int requestId, long? answeredBy, CancellationToken cancellationToken = default)
    {
        var request = await Request(requestId, cancellationToken);

        if (request.Status != ChampionshipJoinRequestStatus.Pending)
            throw new ChampionshipException("The request was already answered.");

        Answer(request, ChampionshipJoinRequestStatus.Refused, answeredBy);
        log.Add(request.ChampionshipId, answeredBy, "Refused the request of {0} to join", request.CommunityId);
        notify.JoinAnswered(await Load(request.ChampionshipId, cancellationToken), request.CommunityId, false);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private void Answer(ChampionshipJoinRequest request, ChampionshipJoinRequestStatus status, long? answeredBy)
    {
        request.Status = status;
        request.AnsweredAtUtc = clock.UtcNow;
        request.AnsweredBy = answeredBy;
    }

    private async Task<ChampionshipJoinRequest> Request(int requestId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampionshipJoinRequests.FirstOrDefaultAsync(item => item.Id == requestId, cancellationToken) ?? throw new ChampionshipException("The request was not found.");
    }

    private async Task<Championship> Load(int championshipId, CancellationToken cancellationToken)
    {
        return await dbContext.Championships.Include(championship => championship.Participants).FirstOrDefaultAsync(championship => championship.Id == championshipId, cancellationToken)
               ?? throw new ChampionshipException("The championship was not found.");
    }

    private static bool IsFull(Championship championship)
    {
        return championship.MaxParticipants != null && championship.Participants.Count >= championship.MaxParticipants;
    }

    private static string? Message(string? message)
    {
        return Text.Clip(message, AppConsts.ChampionshipJoinMessageMaxLength);
    }
}
