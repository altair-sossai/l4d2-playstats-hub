namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityVoter(long CommunityId, string Name, int Votes, DateTime LastVoteAtUtc);