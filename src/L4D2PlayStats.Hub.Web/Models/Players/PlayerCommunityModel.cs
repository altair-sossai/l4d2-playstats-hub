using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Web.Models.Community;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public class PlayerCommunityModel
{
    public required PlayerCommunityStanding First { get; init; }
    public PlayerCommunityStanding? Second { get; init; }
    public IReadOnlyList<CommunityVoteFeedItem> Received { get; init; } = [];
    public IReadOnlyList<CommunityVoteFeedItem> Cast { get; init; } = [];
    public bool HasMoreReceived { get; init; }
    public bool HasMoreCast { get; init; }
    public bool ShowVoters { get; init; }
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Utc;

    public bool ShowsVotes => Second == null;

    public bool HasActivity => First.Rating != null || Received.Count > 0 || Cast.Count > 0;

    public CommunityVoteRowsModel ReceivedRows => new(Received, ShowVoters, TimeZone, null, First.CommunityId);

    public CommunityVoteRowsModel CastRows => new(Cast, ShowVoters, TimeZone, First.CommunityId, null);
}
