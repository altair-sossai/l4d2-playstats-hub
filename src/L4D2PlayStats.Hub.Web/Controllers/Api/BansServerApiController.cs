using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Bans.Models;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Bans;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route(AppConsts.BansApiRoute)]
[RequireModule(HubModule.Bans)]
[GameServerAuthorize]
public class BansServerApiController(IBanService banService, ISiteClock siteClock, IClock clock) : ControllerBase
{
    [HttpGet("{player}")]
    public async Task<IActionResult> GetAsync(string player, CancellationToken cancellationToken)
    {
        var communityId = SteamIdParser.Parse(player);

        if (communityId == null)
            return BadRequest(new { error = AppConsts.BanBadRequestMessage });

        return await ResultAsync(communityId.Value, await banService.ActiveAsync(communityId.Value, cancellationToken), cancellationToken);
    }

    [HttpPost("connections")]
    public async Task<IActionResult> ConnectionAsync([FromBody] GameBanConnection? connection, CancellationToken cancellationToken)
    {
        var communityId = SteamIdParser.Parse(connection?.Player);

        if (communityId == null)
            return BadRequest(new { error = AppConsts.BanBadRequestMessage });

        return await ResultAsync(communityId.Value, await banService.AttemptAsync(communityId.Value, cancellationToken), cancellationToken);
    }

    [HttpPost("")]
    public async Task<IActionResult> PostAsync([FromBody] GameBanCommand? command, CancellationToken cancellationToken)
    {
        var ban = await banService.CaptureAsync(command, HttpContext.GameServer().Name, cancellationToken);

        return ban == null ? BadRequest(new { error = AppConsts.BanBadRequestMessage }) : await ResultAsync(ban.CommunityId, ban, cancellationToken);
    }

    [HttpPost("lift")]
    public async Task<IActionResult> LiftAsync([FromBody] GameUnbanCommand? command, CancellationToken cancellationToken)
    {
        var communityId = SteamIdParser.Parse(command?.Player);

        if (communityId == null)
            return BadRequest(new { error = AppConsts.BanBadRequestMessage });

        await banService.CaptureLiftAsync(command, cancellationToken);

        return await ResultAsync(communityId.Value, null, cancellationToken);
    }

    private async Task<IActionResult> ResultAsync(long communityId, Core.Modules.Bans.Ban? ban, CancellationToken cancellationToken)
    {
        return Ok(new BanServerResult(communityId, ban, clock.UtcNow, await siteClock.ZoneAsync(cancellationToken)));
    }
}
