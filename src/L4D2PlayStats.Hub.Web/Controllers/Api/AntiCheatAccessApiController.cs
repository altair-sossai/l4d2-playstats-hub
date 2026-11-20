using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route(AppConsts.AntiCheatAccessRoute)]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.AntiCheat)]
[ApiController]
public class AntiCheatAccessApiController(
    IAntiCheatAccessService accessService,
    IPlayerLookupService playerLookupService,
    ICurrentUser currentUser,
    IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return Ok(new AntiCheatAccessResult(await accessService.GetAsync(cancellationToken)));
    }

    [HttpPut("mode")]
    public async Task<IActionResult> PutModeAsync([FromBody] AntiCheatAccessModeCommand command, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseMode(command.Mode, out var mode))
            return BadRequest(new { errorMessage = localizer["The mode is invalid."].Value });

        await accessService.SetModeAsync(mode, Actor(), AntiCheatAccessSource.Site, cancellationToken);

        return Ok(new AntiCheatAccessResult(await accessService.GetAsync(cancellationToken)));
    }

    [HttpPost("{list}")]
    public async Task<IActionResult> PostAsync(string list, [FromBody] AntiCheatAccessPlayerCommand command, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        if (string.IsNullOrWhiteSpace(command.Player))
            return BadRequest(new { errorMessage = localizer["The Steam ID must be provided."].Value });

        var communityId = SteamIdParser.Parse(command.Player);
        if (communityId == null)
            return BadRequest(new { errorMessage = localizer["The Steam ID is invalid."].Value });

        var name = string.IsNullOrWhiteSpace(command.Name) ? (await playerLookupService.LookupAsync(command.Player, cancellationToken))?.Name : command.Name;
        var result = await accessService.AddAsync(accessList, communityId.Value, name, Actor(), AntiCheatAccessSource.Site, cancellationToken);

        return result == null
            ? BadRequest(new { errorMessage = localizer["The Steam ID is invalid."].Value })
            : Ok(new AntiCheatAccessPlayerResult(result.Player));
    }

    [HttpDelete("{list}/{communityId}")]
    public async Task<IActionResult> DeleteAsync(string list, string communityId, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        var parsed = SteamIdParser.Parse(communityId);
        if (parsed != null)
            await accessService.RemoveAsync(accessList, parsed.Value, Actor(), AntiCheatAccessSource.Site, cancellationToken);

        return NoContent();
    }

    private AntiCheatAccessActor Actor()
    {
        return new AntiCheatAccessActor(currentUser.CommunityId, currentUser.Name);
    }
}
