namespace L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;

public interface IMatchBuildService
{
    Task RebuildAroundAsync(int gameServerId, int seasonId, DateTime fileDate, CancellationToken cancellationToken = default);
    Task<int> RebuildAllAsync(CancellationToken cancellationToken = default);
}
