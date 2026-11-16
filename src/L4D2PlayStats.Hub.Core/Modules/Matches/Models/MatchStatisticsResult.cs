using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public record MatchStatisticsResult(int FileId, string FileName, ParsedStatistics? Statistic);
