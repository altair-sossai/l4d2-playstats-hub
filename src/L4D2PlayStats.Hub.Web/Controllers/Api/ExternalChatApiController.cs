using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/external-chat")]
[RequireModule(HubModule.LiveServer)]
[GameServerAuthorize]
public class ExternalChatApiController(ILiveDataService liveDataService, ISettingsService settingsService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] long after = 0, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        if (!modules.IsEnabled(HubModule.SiteChat))
            return Ok(Array.Empty<object>());

        return Ok(liveDataService.SiteMessagesAfter(HttpContext.GameServer(), after));
    }
}
