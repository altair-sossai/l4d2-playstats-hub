using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[RequireModule(HubModule.RemoteCommands)]
[Route("admin/remote-commands")]
public class RemoteCommandsController(
    IRemoteCommandService remoteCommandService,
    IGameServerService gameServerService,
    ILiveViewService liveViewService,
    ISiteClock siteClock,
    ICurrentUser currentUser) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var logs = await remoteCommandService.PageAsync(page, AppConsts.RemoteCommandsPageSize, cancellationToken);

        var model = new RemoteCommandsViewModel
        {
            Servers = await liveViewService.EnabledServersAsync(cancellationToken),
            Logs = new PagedListModel<RemoteCommandLog>(logs),
            Delivered = await remoteCommandService.DeliveredCountAsync(cancellationToken),
            TimeZone = await siteClock.ZoneAsync(cancellationToken)
        };

        return View(model);
    }

    [HttpPost("")]
    public async Task<IActionResult> Queue(RemoteCommandForm form, CancellationToken cancellationToken)
    {
        var server = await gameServerService.FindAsync(form.ServerId, cancellationToken);
        var communityId = currentUser.CommunityId;

        if (server is not { Enabled: true } || communityId == null)
            return NotFound();

        if (!await ValidAsync(form, cancellationToken))
            return RedirectToAction(nameof(Index));

        var result = await remoteCommandService.QueueAsync(server, communityId.Value, currentUser.Name, form.Command, cancellationToken);

        if (result.Queued)
            TempData[AppConsts.QueuedTempData] = result.Log!.Command;
        else if (result.Status == RemoteCommandStatus.InvalidCommand)
            Error(AppConsts.InvalidRemoteCommandMessage);
        else
            return NotFound();

        return RedirectToAction(nameof(Index));
    }
}
