using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;

public interface ICommunityVoteService
{
    Task<CommunityBallot> BallotAsync(long voterId, CancellationToken cancellationToken = default);
    Task<CommunityVoteResult> VoteAsync(long voterId, long leftPlayerId, long rightPlayerId, CommunityVoteOutcome outcome, CancellationToken cancellationToken = default);
}
