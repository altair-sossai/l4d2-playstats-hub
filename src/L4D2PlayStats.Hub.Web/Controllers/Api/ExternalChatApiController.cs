using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

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
