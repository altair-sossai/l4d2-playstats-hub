using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[RequireModule(HubModule.LiveServer)]
[Route("admin/player-connections")]
public class PlayerConnectionsController(IPlayerConnectionService playerConnectionService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(long? communityId, CancellationToken cancellationToken)
    {
        var model = new PlayerConnectionsViewModel
        {
            CommunityId = communityId,
            Details = communityId == null ? null : await playerConnectionService.DetailsAsync(communityId.Value, cancellationToken)
        };

        return View(model);
    }
}
