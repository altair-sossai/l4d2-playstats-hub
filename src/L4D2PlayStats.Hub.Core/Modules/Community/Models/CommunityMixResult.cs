using L4D2PlayStats.Hub.Core.Modules.Community.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityMixResult(CommunityMixStatus Status, CommunityMixSplit? Split, IReadOnlyList<long> Unranked);
