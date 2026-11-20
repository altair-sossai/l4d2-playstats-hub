using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;

public interface IStatisticsService
{
    Task<UploadResult> UploadAsync(GameServer server, string? fileName, string? content, CancellationToken cancellationToken = default);
    Task<StatisticsFile?> UpdateScoreAsync(int fileId, int teamAScore, int teamBScore, CancellationToken cancellationToken = default);
    Task<StatisticsFile?> FindAsync(int fileId, CancellationToken cancellationToken = default);
}