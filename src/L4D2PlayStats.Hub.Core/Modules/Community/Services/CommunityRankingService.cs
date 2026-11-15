using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityRankingService(
    HubDbContext dbContext,
    HubState hubState,
    ISettingsService settingsService,
    ICommunityPlayerService playerService) : ICommunityRankingService
{
    public async Task<IReadOnlyList<CommunityRankingEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);

        var ratings = await dbContext.Set<CommunityRating>()
            .AsNoTracking()
            .Where(r => r.Comparisons >= settings.MinimumComparisonsToBeListed)
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.Comparisons)
            .ThenBy(r => r.CommunityId)
            .ToListAsync(cancellationToken);

        var names = await playerService.NamesAsync([.. ratings.Select(r => r.CommunityId)], cancellationToken);

        return [.. ratings.Select((r, index) => new CommunityRankingEntry(index + 1, r.CommunityId, names[r.CommunityId], r.Rating, r.Comparisons, r.Wins, r.Draws, r.Losses))];
    }

    public async Task<IReadOnlyList<CommunityVoter>> VotersAsync(CancellationToken cancellationToken = default)
    {
        var voters = await dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .GroupBy(v => v.VoterId)
            .Select(group => new { VoterId = group.Key, Votes = group.Count(), LastVoteAtUtc = group.Max(v => v.CreatedAtUtc) })
            .OrderByDescending(voter => voter.Votes)
            .ThenBy(voter => voter.VoterId)
            .Take(AppConsts.CommunityVoterListSize)
            .ToListAsync(cancellationToken);

        var names = await playerService.NamesAsync([.. voters.Select(voter => voter.VoterId)], cancellationToken);

        return [.. voters.Select(voter => new CommunityVoter(voter.VoterId, names[voter.VoterId], voter.Votes, voter.LastVoteAtUtc))];
    }

    public async Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        int votes;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            votes = await ReplayAsync(null, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        return votes;
    }

    public async Task<int> RemoveVoterAsync(long voterId, CancellationToken cancellationToken = default)
    {
        int removed;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var votes = await dbContext.Set<CommunityVote>().Where(v => v.VoterId == voterId).ToListAsync(cancellationToken);
            var offers = await dbContext.Set<CommunityOffer>().Where(o => o.VoterId == voterId).ToListAsync(cancellationToken);

            dbContext.RemoveRange(votes);
            dbContext.RemoveRange(offers);

            await ReplayAsync(voterId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            removed = votes.Count;
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        return removed;
    }

    private async Task<int> ReplayAsync(long? withoutVoterId, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var ratings = await dbContext.Set<CommunityRating>().ToListAsync(cancellationToken);

        foreach (var rating in ratings)
            Reset(rating, settings);

        var votes = await dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .Where(v => withoutVoterId == null || v.VoterId != withoutVoterId)
            .OrderBy(v => v.Id)
            .Select(v => new { v.LeftPlayerId, v.RightPlayerId, v.Outcome })
            .ToListAsync(cancellationToken);

        var ledger = new CommunityRatingLedger(settings, ratings);

        foreach (var vote in votes)
            ledger.Apply(vote.LeftPlayerId, vote.RightPlayerId, vote.Outcome);

        dbContext.AddRange(ledger.Created);
        dbContext.RemoveRange(ratings.Where(rating => rating.Comparisons == 0));

        return votes.Count;
    }

    private static void Reset(CommunityRating rating, CommunityRankingSettings settings)
    {
        rating.Rating = settings.InitialRating;
        rating.Comparisons = 0;
        rating.Wins = 0;
        rating.Losses = 0;
        rating.Draws = 0;
    }
}
