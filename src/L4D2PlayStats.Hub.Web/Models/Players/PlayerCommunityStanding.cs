using L4D2PlayStats.Hub.Core.Modules.Community;
using L4D2PlayStats.Hub.Core.Modules.Community.Models;

namespace L4D2PlayStats.Hub.Web.Models.Players;

public record PlayerCommunityStanding(long CommunityId, CommunityRankingEntry? Entry, CommunityRating? Rating);
