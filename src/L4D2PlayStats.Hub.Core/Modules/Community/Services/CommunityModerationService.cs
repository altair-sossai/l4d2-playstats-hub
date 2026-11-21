using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityModerationService(
    HubDbContext dbContext,
    HubState hubState,
    IClock clock,
    ICommunityPlayerService playerService,
    ICommunityRatingService ratingService) : ICommunityModerationService
{
    public Task<bool> IsBlockedAsync(long voterId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CommunityVoterBlock>().AnyAsync(b => b.CommunityId == voterId, cancellationToken);
    }

    public async Task<IReadOnlyList<CommunityBlockedVoter>> BlockedAsync(CancellationToken cancellationToken = default)
    {
        var blocks = await dbContext.Set<CommunityVoterBlock>().AsNoTracking().ToListAsync(cancellationToken);
        var names = await playerService.NamesAsync([.. blocks.Select(b => b.CommunityId)], cancellationToken);

        return
        [
            .. blocks
                .OrderByDescending(b => b.BlockedAtUtc)
                .ThenBy(b => b.CommunityId)
                .Select(b => new CommunityBlockedVoter(b.CommunityId, names[b.CommunityId], b.BlockedAtUtc))
        ];
    }

    public async Task<bool> BlockAsync(long voterId, long? blockedBy, CancellationToken cancellationToken = default)
    {
        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            if (await IsBlockedAsync(voterId, cancellationToken))
                return false;

            var offer = await dbContext.Set<CommunityOffer>().FirstOrDefaultAsync(o => o.VoterId == voterId, cancellationToken);

            if (offer != null)
                dbContext.Remove(offer);

            dbContext.Add(new CommunityVoterBlock { CommunityId = voterId, BlockedAtUtc = clock.UtcNow, BlockedBy = blockedBy });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        return true;
    }

    public async Task<bool> UnblockAsync(long voterId, CancellationToken cancellationToken = default)
    {
        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            return await dbContext.Set<CommunityVoterBlock>().Where(b => b.CommunityId == voterId).ExecuteDeleteAsync(cancellationToken) > 0;
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    public async Task<bool> RemoveVoteAsync(int voteId, CancellationToken cancellationToken = default)
    {
        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var vote = await dbContext.Set<CommunityVote>().FirstOrDefaultAsync(v => v.Id == voteId, cancellationToken);

            if (vote == null)
                return false;

            dbContext.Remove(vote);

            await dbContext.SaveChangesAsync(cancellationToken);

            if (vote.Outcome.IsJudgement())
            {
                await ratingService.RecalculateAsync(null, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        hubState.CommunityChanged();

        return true;
    }
}
