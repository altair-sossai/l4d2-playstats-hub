using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[RequireModule(HubModule.LiveServer)]
public class LegacyGameInfoApiController(ILiveViewService liveViewService) : ControllerBase
{
    [HttpGet("api/game-info")]
    [HttpGet("api/game-info/{section:regex(^(configuration|round|scoreboard|players|messages|events)$)}")]
    public async Task<IActionResult> Get(string? section, CancellationToken cancellationToken)
    {
        var server = await liveViewService.MainServerAsync(cancellationToken);
        if (server == null)
            return NotFound();

        var path = string.IsNullOrEmpty(section) ? string.Empty : $"/{section}";

        return Redirect($"{AppConsts.ServersApiUrl}/{server.Server.PublicId}/game-info{path}{Request.QueryString}");
    }
}
