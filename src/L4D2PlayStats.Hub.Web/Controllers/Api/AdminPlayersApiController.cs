using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Models.Players;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route("api/admin/players")]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[ApiController]
public class AdminPlayersApiController(
    IPlayerLookupService playerLookupService,
    IPlayerDirectoryService playerDirectoryService,
    IRelatedAccountsService relatedAccountsService,
    ISiteClock siteClock) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string? q, [FromQuery] int page, CancellationToken cancellationToken)
    {
        var result = await playerDirectoryService.SearchAsync(q, page, cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);

        return Ok(new PlayerSearchPageResult(result, timeZone));
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> LookupAsync([FromQuery] string? id, CancellationToken cancellationToken)
    {
        var player = await playerLookupService.LookupAsync(id, cancellationToken);

        return player == null ? NotFound() : Ok(player);
    }

    [HttpGet("related")]
    public async Task<IActionResult> RelatedAsync([FromQuery] string? id, [FromQuery] string? list, CancellationToken cancellationToken)
    {
        var related = await relatedAccountsService.FindAsync(id, list, cancellationToken);

        return related == null ? NotFound() : Ok(related);
    }
}
