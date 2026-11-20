using L4D2PlayStats.Hub.Core.Modules.Community.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public class CommunityBallot
{
    public CommunityBallotStatus Status { get; init; }
    public CommunityBallotPlayer? Left { get; init; }
    public CommunityBallotPlayer? Right { get; init; }
    public int Matches { get; init; }
    public int MinimumMatches { get; init; }
    public int VotesLeft { get; init; }
    public int VotesPerDay { get; init; }
}