using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Models;

public record AssemblyFile(StatisticsFile File, ParsedStatistics? Statistics);
