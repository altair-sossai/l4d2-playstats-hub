using L4D2PlayStats.Hub.Core.Modules.Community.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Community;

public class CommunityVote
{
    public int Id { get; set; }
    public long VoterId { get; set; }
    public long LeftPlayerId { get; set; }
    public long RightPlayerId { get; set; }
    public CommunityVoteOutcome Outcome { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime SiteDay { get; set; }
}