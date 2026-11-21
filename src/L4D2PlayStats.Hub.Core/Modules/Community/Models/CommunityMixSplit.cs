namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityMixSplit(IReadOnlyList<long> Survivors, IReadOnlyList<long> Infected, double SurvivorsChance);
