using PageMath = L4D2PlayStats.Hub.Core.Infrastructure.Helpers.Paging;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;

namespace L4D2PlayStats.Hub.Tests.Support.Paging;

public class FakeCommunityRankingService(int count) : ICommunityRankingService
{
    public Task<IReadOnlyList<CommunityRankingEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CommunityRankingEntry> entries =
        [
            .. Enumerable.Range(0, count).Select(index => new CommunityRankingEntry(index + 1, PagingSamples.CommunityId(index), PagingSamples.Name(index), 2000 - index, 40, 10, 5, 2, 3))
        ];

        return Task.FromResult(entries);
    }

    public async Task<Page<CommunityRankingEntry>> PageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var entries = await ListAsync(cancellationToken);
        var number = PageMath.Clamp(page, entries.Count, size);

        return new Page<CommunityRankingEntry>([.. entries.Skip(PageMath.Skip(number, size)).Take(size)], number, size, entries.Count);
    }

    public Task<Page<CommunityVoter>> VotersAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Page<CommunityVoter>.Empty(size));
    }

    public Task<Page<CommunityVoteParticipant>> ParticipantsAsync(bool voters, PlayerSearch search, int page, int size, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Page<CommunityVoteParticipant>.Empty(size));
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

    public Task<Page<CommunityVoteFeedItem>> VotesPageAsync(long? voterId, long? playerId, int page, int size, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Page<CommunityVoteFeedItem>.Empty(size));
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
