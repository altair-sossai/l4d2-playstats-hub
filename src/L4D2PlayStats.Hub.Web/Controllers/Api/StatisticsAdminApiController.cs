using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route(AppConsts.StatisticsApiRoute)]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
public class StatisticsAdminApiController(
    IStatisticsService statisticsService,
    ICurrentUser currentUser,
    ILogger<StatisticsAdminApiController> logger) : ApiControllerBase
{
    // The pencil next to a player on the page of a map: another player takes that line (a survivor or an infected of
    // one round half), with its numbers, for a substitute; the match and the ranking are rebuilt.
    [HttpPost("{fileId:int}/replace-player")]
    public async Task<IActionResult> ReplacePlayerAsync(int fileId, [FromBody] ReplacePlayerCommand command, CancellationToken cancellationToken)
    {
        if (await InvalidAsync(command, cancellationToken) is { } invalid)
            return invalid;

        var replaced = SteamIdParser.Parse(command.Replaced)!.Value;
        var replacement = SteamIdParser.Parse(command.Replacement)!.Value;
        var result = await statisticsService.ReplacePlayerAsync(fileId, command.Half!.Value, command.Survivor!.Value, replaced, replacement, command.ReplacementName, cancellationToken);

        if (!result.Replaced)
            return Invalid(Message(result.Error!.Value));

        logger.LogInformation("The administrator {Administrator} put {Replacement} in the place of {Replaced} in the round half {Half} ({Side}) of the statistics file {File}", currentUser.CommunityId, replacement, replaced, command.Half, command.Survivor == true ? "survivor" : "infected", fileId);

        return Ok(new { matchId = result.MatchId, fileId });
    }

    private static string Message(PlayerReplacementError error)
    {
        return error switch
        {
            PlayerReplacementError.SamePlayer => AppConsts.ReplacementSamePlayerMessage,
            PlayerReplacementError.FileNotFound => AppConsts.ReplacementFileNotFoundMessage,
            PlayerReplacementError.UnreadableFile => AppConsts.ReplacementUnreadableFileMessage,
            PlayerReplacementError.NotInHalf => AppConsts.ReplacementNotInHalfMessage,
            PlayerReplacementError.AlreadyInHalf => AppConsts.ReplacementAlreadyInHalfMessage,
            PlayerReplacementError.OtherTeam => AppConsts.ReplacementOtherTeamMessage,
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };
    }
}
