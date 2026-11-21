using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
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
        var ratings = await (await ListedAsync(cancellationToken)).ToListAsync(cancellationToken);

        return await EntriesAsync(ratings, 0, cancellationToken);
    }

    public async Task<Page<CommunityRankingEntry>> PageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var ratings = await (await ListedAsync(cancellationToken)).ToPageAsync(page, size, cancellationToken);
        var entries = await EntriesAsync(ratings.Items, Paging.Skip(ratings.Number, ratings.Size), cancellationToken);

        return new Page<CommunityRankingEntry>(entries, ratings.Number, ratings.Size, ratings.Total);
    }

    private async Task<IQueryable<CommunityRating>> ListedAsync(CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<CommunityRankingSettings>(cancellationToken);
        var eligible = playerService.Candidates(settings.MinimumMatchesToBeCandidate);
        var minimum = settings.MinimumComparisonsToBeListed;

        var listed = dbContext.Set<CommunityRating>()
            .AsNoTracking()
            .Where(r => r.Comparisons >= minimum && eligible.Contains(r.CommunityId))
            .OrderByDescending(r => r.Rating - AppConsts.CommunityConservativeDeviations * r.Deviation)
            .ThenByDescending(r => r.Comparisons)
            .ThenBy(r => r.CommunityId);

        return settings.MaximumPlayersListed > 0 ? listed.Take(settings.MaximumPlayersListed) : listed;
    }

    private async Task<IReadOnlyList<CommunityRankingEntry>> EntriesAsync(IReadOnlyList<CommunityRating> ratings, int skip, CancellationToken cancellationToken)
    {
        var names = await playerService.NamesAsync([.. ratings.Select(r => r.CommunityId)], cancellationToken);

        return [.. ratings.Select((r, index) => new CommunityRankingEntry(skip + index + 1, r.CommunityId, names[r.CommunityId], r.Rating, r.Deviation, r.Comparisons, r.Wins, r.Draws, r.Losses))];
    }

    public async Task<Page<CommunityVoteFeedItem>> VotesPageAsync(long? voterId, long? playerId, int page, int size, CancellationToken cancellationToken = default)
    {
        var votes = await FilteredVotes(voterId, playerId).ToPageAsync(page, size, cancellationToken);
        var feed = await FeedAsync(votes.Items, cancellationToken);

        return new Page<CommunityVoteFeedItem>(feed, votes.Number, votes.Size, votes.Total);
    }

    private IQueryable<CommunityVote> FilteredVotes(long? voterId, long? playerId)
    {
        return JudgingVotes()
            .Where(v => voterId == null || v.VoterId == voterId)
            .Where(v => playerId == null || v.LeftPlayerId == playerId || v.RightPlayerId == playerId)
            .OrderByDescending(v => v.Id);
    }

    private async Task<IReadOnlyList<CommunityVoteFeedItem>> FeedAsync(IReadOnlyList<CommunityVote> votes, CancellationToken cancellationToken)
    {
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

    public async Task<Page<CommunityVoter>> VotersAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var voters = await dbContext.Set<CommunityVote>()
            .AsNoTracking()
            .GroupBy(v => v.VoterId)
            .Select(group => new { VoterId = group.Key, Votes = group.Count(), LastVoteAtUtc = group.Max(v => v.CreatedAtUtc) })
            .OrderByDescending(voter => voter.Votes)
            .ThenBy(voter => voter.VoterId)
            .ToPageAsync(page, size, cancellationToken);

        List<long> ids = [.. voters.Items.Select(voter => voter.VoterId)];
        var names = await playerService.NamesAsync(ids, cancellationToken);

        var blocked = await dbContext.Set<CommunityVoterBlock>()
            .AsNoTracking()
            .Where(b => ids.Contains(b.CommunityId))
            .Select(b => b.CommunityId)
            .ToListAsync(cancellationToken);

        return voters.Map(voter => new CommunityVoter(voter.VoterId, names[voter.VoterId], voter.Votes, voter.LastVoteAtUtc, blocked.Contains(voter.VoterId)));
    }

    public async Task<Page<CommunityVoteParticipant>> ParticipantsAsync(bool voters, PlayerSearch search, int page, int size, CancellationToken cancellationToken = default)
    {
        var participants = voters
            ? JudgingVotes().GroupBy(v => v.VoterId).Select(group => new CommunityLastVote { CommunityId = group.Key, LastAtUtc = group.Max(v => v.CreatedAtUtc) })
            : JudgingVotes().Select(v => new { CommunityId = v.LeftPlayerId, v.CreatedAtUtc })
                .Concat(JudgingVotes().Select(v => new { CommunityId = v.RightPlayerId, v.CreatedAtUtc }))
                .GroupBy(v => v.CommunityId)
                .Select(group => new CommunityLastVote { CommunityId = group.Key, LastAtUtc = group.Max(v => v.CreatedAtUtc) });

        var profiles = dbContext.PlayerProfiles;
        var connections = dbContext.Set<PlayerConnection>();

        var found = await search
            .Apply(participants, participant => participant.CommunityId, participant =>
                profiles.Where(profile => profile.CommunityId == participant.CommunityId).Select(profile => profile.DisplayName ?? profile.Name).FirstOrDefault()
                ?? connections.Where(connection => connection.CommunityId == participant.CommunityId).OrderByDescending(connection => connection.LastSeenAtUtc).Select(connection => connection.Name).FirstOrDefault())
            .OrderByDescending(participant => participant.LastAtUtc)
            .ThenBy(participant => participant.CommunityId)
            .ToPageAsync(page, size, cancellationToken);

        var names = await playerService.NamesAsync([.. found.Items.Select(participant => participant.CommunityId)], cancellationToken);

        return found.Map(participant => new CommunityVoteParticipant(participant.CommunityId, names[participant.CommunityId], voters ? participant.LastAtUtc : null, voters ? null : participant.LastAtUtc));
    }

    public async Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        int votes;

        await using (await hubState.LockAsync(cancellationToken))
        {
            votes = await ratingService.RecalculateAsync(null, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        hubState.CommunityChanged();

        return votes;
    }

    public async Task<int> ResetAsync(CancellationToken cancellationToken = default)
    {
        int removed;

        await using (await hubState.LockAsync(cancellationToken))
        {
            removed = await dbContext.Set<CommunityVote>().ExecuteDeleteAsync(cancellationToken);

            await dbContext.Set<CommunityOffer>().ExecuteDeleteAsync(cancellationToken);
            await dbContext.Set<CommunityRating>().ExecuteDeleteAsync(cancellationToken);
        }

        hubState.CommunityChanged();

        return removed;
    }

    public async Task<int> RemoveVoterAsync(long voterId, CancellationToken cancellationToken = default)
    {
        int removed;

        await using (await hubState.LockAsync(cancellationToken))
        {
            var votes = await dbContext.Set<CommunityVote>().Where(v => v.VoterId == voterId).ToListAsync(cancellationToken);
            var offers = await dbContext.Set<CommunityOffer>().Where(o => o.VoterId == voterId).ToListAsync(cancellationToken);

            dbContext.RemoveRange(votes);
            dbContext.RemoveRange(offers);

            await ratingService.RecalculateAsync(voterId, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            removed = votes.Count;
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
