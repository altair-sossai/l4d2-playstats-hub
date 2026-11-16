using L4D2PlayStats.Hub.Core.Modules.Community.Models;
using L4D2PlayStats.Hub.Core.Modules.Patents;

namespace L4D2PlayStats.Hub.Web.Models.Community;

public record CommunityRankingRow(CommunityRankingEntry Entry, Patent? Patent);
