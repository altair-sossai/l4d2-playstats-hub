using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/external-chat")]
[RequireModule(HubModule.SiteChat)]
[GameServerAuthorize]
public class ExternalChatApiController(ILiveDataService liveDataService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromQuery] long after = 0)
    {
        return Ok(liveDataService.SiteMessagesAfter(HttpContext.GameServer(), after));
    }
}
