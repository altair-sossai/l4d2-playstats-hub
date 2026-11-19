using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[RequireModule(HubModule.LiveServer)]
[Route("admin/player-connections")]
public class PlayerConnectionsController(IPlayerConnectionService playerConnectionService, ILiveViewService liveViewService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? communityId, CancellationToken cancellationToken)
    {
        ViewBag.Admin = "active";
        ViewBag.Administration = "active";

        var parsed = SteamIdParser.Parse(communityId);

        var model = new PlayerConnectionsViewModel
        {
            Query = communityId?.Trim(),
            CommunityId = parsed,
            Details = parsed == null ? null : await playerConnectionService.DetailsAsync(parsed.Value, cancellationToken),
            TimeZone = await liveViewService.TimeZoneAsync(cancellationToken)
        };

        return View(model);
    }
}
