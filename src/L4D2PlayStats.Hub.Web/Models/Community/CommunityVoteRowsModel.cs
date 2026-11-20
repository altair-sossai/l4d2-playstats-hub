using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public record CommunityVoteRowsModel(IReadOnlyList<CommunityVoteFeedItem> Votes, bool ShowVoters, TimeZoneInfo TimeZone, long? VoterId, long? PlayerId);
