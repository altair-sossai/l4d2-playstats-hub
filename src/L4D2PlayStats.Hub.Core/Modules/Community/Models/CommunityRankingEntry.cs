namespace L4D2PlayStats.Hub.Core.Modules.Community.Models;

public record CommunityRankingEntry(int Position, long CommunityId, string Name, double Rating, int Comparisons, int Wins, int Draws, int Losses);