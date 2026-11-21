namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityMixOption(IReadOnlyList<CommunityMixPlayer> TeamA, IReadOnlyList<CommunityMixPlayer> TeamB, double Cost, string Key);
