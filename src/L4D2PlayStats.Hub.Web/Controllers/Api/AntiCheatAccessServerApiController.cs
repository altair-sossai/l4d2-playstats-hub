using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[Route(AppConsts.AntiCheatAccessServerRoute)]
[RequireModule(HubModule.AntiCheat)]
[GameServerAuthorize]
public class AntiCheatAccessServerApiController(IAntiCheatAccessService accessService) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        return await SnapshotAsync(cancellationToken);
    }

    [HttpPut("mode")]
    public async Task<IActionResult> PutModeAsync([FromBody] AntiCheatAccessModeCommand command, CancellationToken cancellationToken)
    {
        if (await InvalidAsync(command, cancellationToken) is { } invalid)
            return invalid;

        AntiCheatAccessNames.TryParseMode(command.Mode, out var mode);

        await accessService.SetModeAsync(mode, Actor(command.AdminCommunityId, command.AdminName), AntiCheatAccessSource.Game, cancellationToken);

        return await SnapshotAsync(cancellationToken);
    }

    [HttpPost("{list}")]
    public async Task<IActionResult> PostAsync(string list, [FromBody] AntiCheatAccessPlayerCommand command, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        if (await InvalidAsync(command, cancellationToken) is { } invalid)
            return invalid;

        var communityId = SteamIdParser.Parse(command.Player)!.Value;
        var result = await accessService.AddAsync(accessList, communityId, command.Name, Actor(command.AdminCommunityId, command.AdminName), AntiCheatAccessSource.Game, cancellationToken);

        if (result == null)
            return Invalid(AppConsts.SteamIdInvalidMessage);

        return await SnapshotAsync(cancellationToken);
    }

    [HttpDelete("{list}")]
    public async Task<IActionResult> DeleteAsync(string list, [FromQuery] string? player, CancellationToken cancellationToken)
    {
        if (!AntiCheatAccessNames.TryParseList(list, out var accessList))
            return NotFound();

        var communityId = SteamIdParser.Parse(player);
        if (communityId == null)
            return Invalid(AppConsts.SteamIdInvalidMessage);

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
