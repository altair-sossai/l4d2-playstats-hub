using L4D2PlayStats.Hub.Core.Modules.Community.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityVoteFeedItem(long Id, long VoterId, string VoterName, long LeftPlayerId, string LeftName, long RightPlayerId, string RightName, CommunityVoteOutcome Outcome, DateTime CreatedAtUtc);