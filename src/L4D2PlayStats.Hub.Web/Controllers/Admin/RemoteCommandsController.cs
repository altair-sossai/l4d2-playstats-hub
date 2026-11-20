using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.RemoteCommands)]
[Route("admin/remote-commands")]
public class RemoteCommandsController(
    IRemoteCommandService remoteCommandService,
    IGameServerService gameServerService,
    ILiveViewService liveViewService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Admin = "active";
        ViewBag.Administration = "active";

        var size = AppConsts.RemoteCommandsPageSize;
        var total = await remoteCommandService.CountAsync(cancellationToken: cancellationToken);
        var current = Paging.Page(page, total, size);
        var logs = await remoteCommandService.ListAsync(Paging.Skip(current, size), size, cancellationToken);

        var model = new RemoteCommandsViewModel
        {
            Servers = await liveViewService.EnabledServersAsync(cancellationToken),
            Logs = new PagedListModel<RemoteCommandLog>(logs, current, size, total),
            Delivered = await remoteCommandService.CountAsync(true, cancellationToken),
            TimeZone = await liveViewService.TimeZoneAsync(cancellationToken)
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