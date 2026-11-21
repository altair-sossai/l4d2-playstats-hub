using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public class CommunityVotesViewModel
{
    public Page<CommunityVoteFeedItem> Votes { get; init; } = Page<CommunityVoteFeedItem>.Empty(CoreConsts.CommunityVotesPageSize);
    public long? VoterId { get; init; }
    public string? VoterName { get; init; }
    public long? PlayerId { get; init; }
    public string? PlayerName { get; init; }
    public CommunityRating? PlayerRating { get; init; }
    public IReadOnlyList<CommunityVoteParticipant> Participants { get; init; } = [];
    public bool ShowVoters { get; init; }
    public bool Moderate { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;
}
