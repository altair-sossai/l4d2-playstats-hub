using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityVotesViewModel
{
    public IReadOnlyList<CommunityVoteFeedItem> Votes { get; init; } = [];
    public long? VoterId { get; init; }
    public string? VoterName { get; init; }
    public long? PlayerId { get; init; }
    public string? PlayerName { get; init; }
    public CommunityRating? PlayerRating { get; init; }
    public IReadOnlyList<CommunityVoteParticipant> Participants { get; init; } = [];
    public int Page { get; init; }
    public bool HasMore { get; init; }
    public bool ShowVoters { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
}