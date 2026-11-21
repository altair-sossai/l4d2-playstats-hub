using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Api;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route(AppConsts.CommunityMixRoute)]
[RequireModule(HubModule.CommunityRanking)]
[GameServerAuthorize]
public class CommunityMixApiController(ICommunityMixService mixService, ILogger<CommunityMixApiController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CommunityMixCommand? command, CancellationToken cancellationToken)
    {
        var players = command?.Players ?? [];
        List<long> communityIds = [.. players.Select(player => long.TryParse(player, out var communityId) ? communityId : 0)];

        var result = await mixService.BalanceAsync(communityIds, cancellationToken);

        switch (result.Status)
        {
            case CommunityMixStatus.Invalid:
                return BadRequest(new { error = AppConsts.CommunityMixInvalidError });
            case CommunityMixStatus.Unranked:
                return Conflict(new { error = AppConsts.CommunityMixUnrankedError, players = result.Unranked.Select(communityId => communityId.ToString()) });
        }

        var split = result.Split!;

        logger.LogInformation("Ranking mix on {GameServer} with {Players} players: the survivors have a {Chance:P0} chance", HttpContext.GameServer().Name, communityIds.Count, split.SurvivorsChance);

        return Ok(new
        {
            survivors = split.Survivors.Select(communityId => communityId.ToString()),
            infected = split.Infected.Select(communityId => communityId.ToString()),
            survivorsChance = Math.Round(split.SurvivorsChance, 3)
        });
    }
}
