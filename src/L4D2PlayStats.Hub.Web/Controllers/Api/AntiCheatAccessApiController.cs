using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route(AppConsts.AntiCheatAccessRoute)]
[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.AntiCheat)]
public class AntiCheatAccessApiController(
    IAntiCheatAccessService accessService,
    IPlayerLookupService playerLookupService,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return Ok(new AntiCheatAccessResult(await accessService.GetAsync(cancellationToken)));
    }

    [HttpPut("mode")]
    public async Task<IActionResult> PutModeAsync([FromBody] AntiCheatAccessModeCommand command, CancellationToken cancellationToken)
    {
        if (await InvalidAsync(command, cancellationToken) is { } invalid)
            return invalid;

        AntiCheatAccessNames.TryParseMode(command.Mode, out var mode);

        await accessService.SetModeAsync(mode, Actor(), AntiCheatAccessSource.Site, cancellationToken);

        return Ok(new AntiCheatAccessResult(await accessService.GetAsync(cancellationToken)));
    }

    [HttpPost("{list}")]
    public async Task<IActionResult> PostAsync(string list, [FromBody] AntiCheatAccessPlayerCommand command, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        if (await InvalidAsync(command, cancellationToken) is { } invalid)
            return invalid;

        var communityId = SteamIdParser.Parse(command.Player)!.Value;
        var name = string.IsNullOrWhiteSpace(command.Name) ? (await playerLookupService.LookupAsync(command.Player, cancellationToken))?.Name : command.Name;
        var result = await accessService.AddAsync(accessList, communityId, name, Actor(), AntiCheatAccessSource.Site, cancellationToken);

        return result == null ? Invalid(AppConsts.SteamIdInvalidMessage) : Ok(new AntiCheatAccessPlayerResult(result.Player));
    }

    [HttpDelete("{list}/{communityId}")]
    public async Task<IActionResult> DeleteAsync(string list, string communityId, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        var parsed = SteamIdParser.Parse(communityId);
        if (parsed == null)
            return Invalid(AppConsts.SteamIdInvalidMessage);

        await accessService.RemoveAsync(accessList, parsed.Value, Actor(), AntiCheatAccessSource.Site, cancellationToken);

        return NoContent();
    }

    private AntiCheatAccessActor Actor()
    {
        return new AntiCheatAccessActor(currentUser.CommunityId, currentUser.Name);
    }
}
