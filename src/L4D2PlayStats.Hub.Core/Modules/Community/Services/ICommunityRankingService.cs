using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services;

public interface ICommunityRankingService
{
    Task<IReadOnlyList<CommunityRankingEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunityVoter>> VotersAsync(CancellationToken cancellationToken = default);
    Task<int> RebuildAsync(CancellationToken cancellationToken = default);
    Task<int> RemoveVoterAsync(long voterId, CancellationToken cancellationToken = default);
    Task<int> ResetAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunityVoteFeedItem>> LatestVotesAsync(long? voterId, long? playerId, int skip, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CommunityVoteParticipant>> VoteParticipantsAsync(CancellationToken cancellationToken = default);
    Task<CommunityRating?> RatingAsync(long communityId, CancellationToken cancellationToken = default);
}
