using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[RequireModule(HubModule.LiveServer)]
[Route("admin/player-connections")]
public class PlayerConnectionsController(IPlayerConnectionService playerConnectionService, ISiteClock siteClock) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? communityId, CancellationToken cancellationToken)
    {
        var parsed = SteamIdParser.Parse(communityId);

        var model = new PlayerConnectionsViewModel
        {
            Query = communityId?.Trim(),
            CommunityId = parsed,
            Details = parsed == null ? null : await playerConnectionService.DetailsAsync(parsed.Value, cancellationToken),
            TimeZone = await siteClock.ZoneAsync(cancellationToken)
        };

        return View(model);
    }
}
