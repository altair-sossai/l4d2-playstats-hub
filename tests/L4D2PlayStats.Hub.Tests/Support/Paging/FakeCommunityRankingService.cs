using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Support.Paging;

public class FakeCommunityRankingService(int count) : ICommunityRankingService
{
    public Task<IReadOnlyList<CommunityRankingEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CommunityRankingEntry> entries =
        [
            .. Enumerable.Range(0, count).Select(index => new CommunityRankingEntry(index + 1, PagingSamples.CommunityId(index), PagingSamples.Name(index), 2000 - index, 10, 5, 2, 3))
        ];

        return Task.FromResult(entries);
    }

    public Task<IReadOnlyList<CommunityVoter>> VotersAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<CommunityVoter>>([]);
    }

    public Task<int> VoterCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<int> RemoveVoterAsync(long voterId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<int> ResetAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<IReadOnlyList<CommunityVoteFeedItem>> LatestVotesAsync(long? voterId, long? playerId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<CommunityVoteFeedItem>>([]);
    }

    public Task<IReadOnlyList<CommunityVoteParticipant>> VoteParticipantsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<CommunityVoteParticipant>>([]);
    }

    public Task<CommunityRating?> RatingAsync(long communityId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<CommunityRating?>(null);
    }
}