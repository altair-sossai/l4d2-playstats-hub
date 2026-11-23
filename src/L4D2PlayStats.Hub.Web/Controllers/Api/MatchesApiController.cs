using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route(AppConsts.MatchesApiRoute)]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
public class MatchesApiController(
    IStatisticsService statisticsService,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer,
    ILogger<MatchesApiController> logger) : ApiControllerBase
{
    // A substitute came in for a player: in the chosen maps, the player who came in takes the place and the
    // numbers of the one who left, and the match and the ranking are rebuilt from the changed files.
    [HttpPost("{id:int}/replace-player")]
    public async Task<IActionResult> ReplacePlayerAsync(int id, [FromBody] ReplacePlayerCommand command, CancellationToken cancellationToken)
    {
        if (await InvalidAsync(command, cancellationToken) is { } invalid)
            return invalid;

        var replaced = SteamIdParser.Parse(command.Replaced)!.Value;
        var replacement = SteamIdParser.Parse(command.Replacement)!.Value;
        var result = await statisticsService.ReplacePlayerAsync(id, command.FileIds!, replaced, replacement, command.ReplacementName, cancellationToken);

        if (!result.Replaced)
            return BadRequest(new ApiErrorResult(string.Format(localizer[Message(result.Error!.Value)].Value, result.MapName)));

        logger.LogInformation("The administrator {Administrator} put {Replacement} in the place of {Replaced} in {Maps} map(s) of the match {Match}", currentUser.CommunityId, replacement, replaced, command.FileIds!.Distinct().Count(), id);

        return Ok(new { matchId = result.MatchId });
    }

    private static string Message(PlayerReplacementError error)
    {
        return error switch
        {
            PlayerReplacementError.SamePlayer => AppConsts.ReplacementSamePlayerMessage,
            PlayerReplacementError.NoMaps => AppConsts.ReplacementMapsRequiredMessage,
            PlayerReplacementError.MatchNotFound => AppConsts.ReplacementMatchNotFoundMessage,
            PlayerReplacementError.MapNotInMatch => AppConsts.ReplacementMapNotInMatchMessage,
            PlayerReplacementError.UnreadableMap => AppConsts.ReplacementUnreadableMapMessage,
            PlayerReplacementError.NotInMap => AppConsts.ReplacementNotInMapMessage,
            PlayerReplacementError.AlreadyInMap => AppConsts.ReplacementAlreadyInMapMessage,
            _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
        };
    }
}
