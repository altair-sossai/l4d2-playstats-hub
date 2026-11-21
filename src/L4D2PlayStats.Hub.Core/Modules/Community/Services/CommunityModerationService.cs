using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
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

    public async Task<Page<CommunityBlockedVoter>> BlockedAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var blocks = await dbContext.Set<CommunityVoterBlock>()
            .AsNoTracking()
            .OrderByDescending(b => b.BlockedAtUtc)
            .ThenBy(b => b.CommunityId)
            .ToPageAsync(page, size, cancellationToken);

        var names = await playerService.NamesAsync([.. blocks.Items.Select(b => b.CommunityId)], cancellationToken);

        return blocks.Map(b => new CommunityBlockedVoter(b.CommunityId, names[b.CommunityId], b.BlockedAtUtc));
    }

    public async Task<bool> BlockAsync(long voterId, long? blockedBy, CancellationToken cancellationToken = default)
    {
        await using (await hubState.LockAsync(cancellationToken))
        {
            if (await IsBlockedAsync(voterId, cancellationToken))
                return false;

            var offer = await dbContext.Set<CommunityOffer>().FirstOrDefaultAsync(o => o.VoterId == voterId, cancellationToken);

            if (offer != null)
                dbContext.Remove(offer);

            dbContext.Add(new CommunityVoterBlock { CommunityId = voterId, BlockedAtUtc = clock.UtcNow, BlockedBy = blockedBy });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<bool> UnblockAsync(long voterId, CancellationToken cancellationToken = default)
    {
        await using (await hubState.LockAsync(cancellationToken))
        {
            return await dbContext.Set<CommunityVoterBlock>().Where(b => b.CommunityId == voterId).ExecuteDeleteAsync(cancellationToken) > 0;
        }
    }

    public async Task<bool> RemoveVoteAsync(int voteId, CancellationToken cancellationToken = default)
    {
        await using (await hubState.LockAsync(cancellationToken))
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

        hubState.CommunityChanged();

        return true;
    }
}
