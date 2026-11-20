using L4D2PlayStats.Hub.Web.Models.Api;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Exceptions;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;
using ParsedStatistics = L4D2PlayStats.Statistics;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/statistics")]
public class StatisticsApiController(IStatisticsService statisticsService) : ControllerBase
{
    [HttpPost]
    [GameServerAuthorize]
    public async Task<IActionResult> Upload([FromBody] StatisticsCommand? command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await statisticsService.UploadAsync(HttpContext.GameServer(), command?.FileName, command?.Content, cancellationToken);

            return Ok(new { fileName = result.FileName, mustBeDeleted = result.MustBeDeleted });
        }
        catch (StatisticsRejectedException exception)
        {
            return BadRequest(new { error = exception.Message });
        }
    }

    [HttpPut("{fileId:int}/update-score")]
    [Authorize(Policy = AppConsts.AdministratorPolicy)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateScore(int fileId, [FromBody] UpdateTeamScoreCommand command, CancellationToken cancellationToken)
    {
        var file = await statisticsService.FindAsync(fileId, cancellationToken);
        if (file == null)
            return NotFound(new { error = $"Statistic with ID {fileId} not found." });

        if (!ParsedStatistics.TryParse(file.Content, out var statistics) || statistics?.Scoring?.TeamA == null || statistics.Scoring.TeamB == null)
            return BadRequest(new { error = "Statistic does not have scoring information." });

        if (command.Team != CoreConsts.TeamACode && command.Team != CoreConsts.TeamBCode)
            return BadRequest(new { error = "Invalid team." });

        if (command.Score < AppConsts.MinScore || command.Score > CoreConsts.MaxScore)
            return BadRequest(new { error = "Invalid score range." });

        var teamAScore = command.Team == CoreConsts.TeamACode ? command.Score : statistics.Scoring.TeamA.Score;
        var teamBScore = command.Team == CoreConsts.TeamBCode ? command.Score : statistics.Scoring.TeamB.Score;

        var updated = await statisticsService.UpdateScoreAsync(fileId, teamAScore, teamBScore, cancellationToken);
        if (updated == null)
            return BadRequest(new { error = "The score could not be updated." });

        var current = await statisticsService.FindAsync(fileId, cancellationToken);

        return Ok(new { statisticId = fileId, fileName = file.FileName, matchId = current?.MatchId, teamAScore, teamBScore });
    }
}
