using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services;

public class StatisticsService(
    HubDbContext dbContext,
    HubState hubState,
    ISeasonService seasonService,
    IGameModeService gameModeService,
    IMatchBuildService matchBuildService,
    IGameServerPresence presence,
    IClock clock) : IStatisticsService
{
    public async Task<UploadResult> UploadAsync(GameServer server, string? fileName, string? content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new StatisticsRejectedException("Invalid file name");

        if (string.IsNullOrEmpty(content) || !ParsedStatistics.TryParse(content, out var statistics) || statistics == null)
            throw new StatisticsRejectedException("Invalid content");

        var fileDate = StatisticsFileName.ToDate(fileName);

        if (fileDate == null || !StatisticsContent.IsComplete(statistics))
        {
            if (fileDate == null || fileDate < clock.UtcNow.AddHours(-AppConsts.UploadRetryWindowInHours))
                return new UploadResult(fileName, true);

            throw new StatisticsRejectedException("Incomplete statistics");
        }

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var season = await seasonService.GetOrCreateAsync(fileDate.Value, cancellationToken);
            var file = await dbContext.StatisticsFiles.FirstOrDefaultAsync(f => f.GameServerId == server.Id && f.FileName == fileName, cancellationToken);

            if (file == null)
            {
                file = new StatisticsFile { GameServerId = server.Id, FileName = fileName };
                dbContext.StatisticsFiles.Add(file);
            }

            file.FileDate = fileDate.Value;
            file.SeasonId = season.Id;
            file.Content = content;
            file.UploadedAtUtc = clock.UtcNow;
            file.Apply(statistics);

            await gameModeService.RegisterSeenAsync(file.ConfigurationName, file.TeamSize, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await matchBuildService.RebuildAroundAsync(server.Id, season.Id, file.FileDate, cancellationToken);
        }
        finally
        {
            hubState.WriteLock.Release();
        }

        presence.StatisticsReceived(server.Id);

        return new UploadResult(fileName, true);
    }

    public async Task<StatisticsFile?> UpdateScoreAsync(int fileId, int teamAScore, int teamBScore, CancellationToken cancellationToken = default)
    {
        if (teamAScore < 0 || teamAScore > AppConsts.MaxScore || teamBScore < 0 || teamBScore > AppConsts.MaxScore)
            return null;

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var file = await dbContext.StatisticsFiles.FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
            if (file == null)
                return null;

            if (!ParsedStatistics.TryParse(file.Content, out var statistics) || statistics?.Scoring?.TeamA == null || statistics.Scoring.TeamB == null)
                return null;

            statistics.Scoring.TeamA.Score = teamAScore;
            statistics.Scoring.TeamB.Score = teamBScore;

            file.Content = statistics.ToString();
            file.Apply(statistics);

            await dbContext.SaveChangesAsync(cancellationToken);
            await matchBuildService.RebuildAroundAsync(file.GameServerId, file.SeasonId, file.FileDate, cancellationToken);

            return file;
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    public Task<StatisticsFile?> FindAsync(int fileId, CancellationToken cancellationToken = default)
    {
        return dbContext.StatisticsFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fileId, cancellationToken);
    }
}
