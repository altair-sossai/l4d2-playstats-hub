using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.RemoteCommands)]
[Route("admin/remote-commands")]
public class RemoteCommandsController(IRemoteCommandService remoteCommandService, IGameServerService gameServerService, ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var servers = await gameServerService.ListAsync(cancellationToken);

        var model = new RemoteCommandsViewModel
        {
            Servers = [.. servers.Where(server => server.Enabled)],
            Logs = await remoteCommandService.ListAsync(AppConsts.RemoteCommandLogSize, cancellationToken)
        };

        return View(model);
    }

    [HttpPost("")]
    public async Task<IActionResult> Queue(int serverId, string? command, CancellationToken cancellationToken)
    {
        var server = await gameServerService.FindAsync(serverId, cancellationToken);
        var communityId = currentUser.CommunityId;

        if (server is not { Enabled: true } || communityId == null)
            return NotFound();

        var result = await remoteCommandService.QueueAsync(server, communityId.Value, currentUser.Name, command, cancellationToken);

        if (result.Queued)
            TempData["Queued"] = result.Log!.Command;
        else if (result.Status == RemoteCommandStatus.InvalidCommand)
            TempData["Error"] = "That is not a valid command. Use sm_command, !command or /command, without semicolons.";
        else
            return NotFound();

        return RedirectToAction(nameof(Index));
    }
}
