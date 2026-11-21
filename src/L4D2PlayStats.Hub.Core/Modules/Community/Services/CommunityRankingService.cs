using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public class CommunityRankingService(
    HubDbContext dbContext,
    HubState hubState,
    ISettingsService settingsService,
    ICommunityPlayerService playerService,
    ICommunityRatingService ratingService) : ICommunityRankingService
{
    public async Task<IReadOnlyList<CommunityRankingEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var eligible = await playerService.CandidatesAsync(settings.MinimumMatchesToBeCandidate, cancellationToken: cancellationToken);

        var rated = await dbContext.Set<CommunityRating>()
            .AsNoTracking()
            .Where(r => r.Comparisons >= settings.MinimumComparisonsToBeListed)
            .ToListAsync(cancellationToken);

        var listed = rated
            .Where(r => eligible.Contains(r.CommunityId))
            .OrderByDescending(r => new CommunityStrength(r.Rating, r.Deviation).Conservative)
            .ThenByDescending(r => r.Comparisons)
            .ThenBy(r => r.CommunityId);
        var ratings = (settings.MaximumPlayersListed > 0 ? listed.Take(settings.MaximumPlayersListed) : listed).ToList();

        var names = await playerService.NamesAsync([.. ratings.Select(r => r.CommunityId)], cancellationToken);

        return [.. ratings.Select((r, index) => new CommunityRankingEntry(index + 1, r.CommunityId, names[r.CommunityId], r.Rating, r.Deviation, r.Comparisons, r.Wins, r.Draws, r.Losses))];
    }

    public async Task<IReadOnlyList<CommunityVoteFeedItem>> LatestVotesAsync(long? voterId, long? playerId, int skip, int take, CancellationToken cancellationToken = default)
    {
        var votes = await JudgingVotes()
            .Where(v => voterId == null || v.VoterId == voterId)
            .Where(v => playerId == null || v.LeftPlayerId == playerId || v.RightPlayerId == playerId)
            .OrderByDescending(v => v.Id)
            .Skip(Math.Max(0, skip))
            .Take(Math.Max(0, take))
            .ToListAsync(cancellationToken);

        var names = await playerService.NamesAsync([.. votes.SelectMany(v => new[] { v.VoterId, v.LeftPlayerId, v.RightPlayerId })], cancellationToken);

        return [.. votes.Select(v => new CommunityVoteFeedItem(v.Id, v.VoterId, names[v.VoterId], v.LeftPlayerId, names[v.LeftPlayerId], v.RightPlayerId, names[v.RightPlayerId], v.Outcome, v.CreatedAtUtc))];
    }

    public Task<CommunityRating?> RatingAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CommunityRating>().AsNoTracking().FirstOrDefaultAsync(r => r.CommunityId == communityId, cancellationToken);
    }

    public async Task<IReadOnlyList<CommunityVoteParticipant>> VoteParticipantsAsync(CancellationToken cancellationToken = default)
    {
        var voters = await JudgingVotes()
            .GroupBy(v => v.VoterId)
            .Select(group => new { CommunityId = group.Key, Last = group.Max(v => v.CreatedAtUtc) })
            .ToDictionaryAsync(entry => entry.CommunityId, entry => entry.Last, cancellationToken);

        var lefts = await JudgingVotes()
            .GroupBy(v => v.LeftPlayerId)
            .Select(group => new { CommunityId = group.Key, Last = group.Max(v => v.CreatedAtUtc) })
            .ToListAsync(cancellationToken);

        var rights = await JudgingVotes()
            .GroupBy(v => v.RightPlayerId)
            .Select(group => new { CommunityId = group.Key, Last = group.Max(v => v.CreatedAtUtc) })
            .ToListAsync(cancellationToken);

        var compared = lefts.Concat(rights)
            .GroupBy(entry => entry.CommunityId)
            .ToDictionary(group => group.Key, group => group.Max(entry => entry.Last));

        var names = await playerService.NamesAsync([.. voters.Keys.Concat(compared.Keys)], cancellationToken);

        return
        [
            .. names
                .Select(pair => new CommunityVoteParticipant(pair.Key, pair.Value, voters.TryGetValue(pair.Key, out var voted) ? voted : null, compared.TryGetValue(pair.Key, out var judged) ? judged : null))
                .OrderBy(participant => participant.Name, StringComparer.CurrentCultureIgnoreCase)
        ];
    }

    public async Task<IReadOnlyList<CommunityVoter>> VotersAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var voters = await dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .GroupBy(v => v.VoterId)
            .Select(group => new { VoterId = group.Key, Votes = group.Count(), LastVoteAtUtc = group.Max(v => v.CreatedAtUtc) })
            .OrderByDescending(voter => voter.Votes)
            .ThenBy(voter => voter.VoterId)
            .Skip(Math.Max(0, skip))
            .Take(Math.Max(0, take))
            .ToListAsync(cancellationToken);

        List<long> ids = [.. voters.Select(voter => voter.VoterId)];
        var names = await playerService.NamesAsync(ids, cancellationToken);

        var blocked = await dbContext.Set<CommunityVoterBlock>()
            .AsNoTracking()
            .Where(b => ids.Contains(b.CommunityId))
            .Select(b => b.CommunityId)
            .ToListAsync(cancellationToken);

        return [.. voters.Select(voter => new CommunityVoter(voter.VoterId, names[voter.VoterId], voter.Votes, voter.LastVoteAtUtc, blocked.Contains(voter.VoterId)))];
    }

    public Task<int> VoterCountAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .Select(v => v.VoterId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        int votes;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            votes = await ratingService.RecalculateAsync(null, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        hubState.CommunityChanged();

        return votes;
    }

    public async Task<int> ResetAsync(CancellationToken cancellationToken = default)
    {
        int removed;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            removed = await dbContext.Set<CommunityVote>().ExecuteDeleteAsync(cancellationToken);

            await dbContext.Set<CommunityOffer>().ExecuteDeleteAsync(cancellationToken);
            await dbContext.Set<CommunityRating>().ExecuteDeleteAsync(cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        hubState.CommunityChanged();

        return removed;
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

            await ratingService.RecalculateAsync(voterId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            removed = votes.Count;
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        hubState.CommunityChanged();

        return removed;
    }

    private IQueryable<CommunityVote> JudgingVotes()
    {
        return dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .Where(v => v.Outcome == CommunityVoteOutcome.Left || v.Outcome == CommunityVoteOutcome.Right || v.Outcome == CommunityVoteOutcome.SameLevel);
    }
}
