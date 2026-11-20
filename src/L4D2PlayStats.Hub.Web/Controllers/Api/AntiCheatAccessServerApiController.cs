using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route(AppConsts.AntiCheatAccessServerRoute)]
[RequireModule(HubModule.AntiCheat)]
[GameServerAuthorize]
public class AntiCheatAccessServerApiController(IAntiCheatAccessService accessService, IStringLocalizer<SharedResource> localizer) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return await SnapshotAsync(cancellationToken);
    }

    [HttpPut("mode")]
    public async Task<IActionResult> PutModeAsync([FromBody] AntiCheatAccessModeCommand command, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseMode(command.Mode, out var mode))
            return BadRequest(new { errorMessage = localizer["The mode is invalid."].Value });

        await accessService.SetModeAsync(mode, Actor(command.AdminCommunityId, command.AdminName), AntiCheatAccessSource.Game, cancellationToken);

        return await SnapshotAsync(cancellationToken);
    }

    [HttpPost("{list}")]
    public async Task<IActionResult> PostAsync(string list, [FromBody] AntiCheatAccessPlayerCommand command, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        if (string.IsNullOrWhiteSpace(command.Player))
            return BadRequest(new { errorMessage = localizer["The Steam ID must be provided."].Value });

        var communityId = SteamIdParser.Parse(command.Player);
        var result = communityId == null ? null : await accessService.AddAsync(accessList, communityId.Value, command.Name, Actor(command.AdminCommunityId, command.AdminName), AntiCheatAccessSource.Game, cancellationToken);

        if (result == null)
            return BadRequest(new { errorMessage = localizer["The Steam ID is invalid."].Value });

        return await SnapshotAsync(cancellationToken);
    }

    [HttpDelete("{list}")]
    public async Task<IActionResult> DeleteAsync(string list, [FromQuery] string? player, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        var communityId = SteamIdParser.Parse(player);
        if (communityId == null)
            return BadRequest(new { errorMessage = localizer["The Steam ID is invalid."].Value });

        await accessService.RemoveAsync(accessList, communityId.Value, new AntiCheatAccessActor(null, null), AntiCheatAccessSource.Game, cancellationToken);

        return await SnapshotAsync(cancellationToken);
    }

    private async Task<IActionResult> SnapshotAsync(CancellationToken cancellationToken)
    {
        return Ok(new AntiCheatAccessServerResult(await accessService.GetAsync(cancellationToken)));
    }

    private static AntiCheatAccessActor Actor(string? communityId, string? name)
    {
        return new AntiCheatAccessActor(SteamIdParser.Parse(communityId), name);
    }
}
