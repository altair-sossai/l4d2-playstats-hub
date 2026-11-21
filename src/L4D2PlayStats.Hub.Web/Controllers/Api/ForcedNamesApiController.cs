using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route(AppConsts.ForcedNamesApiRoute)]
[GameServerAuthorize]
public class ForcedNamesApiController(IForcedNameStore forcedNames) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(forcedNames.All
            .OrderBy(entry => entry.Key)
            .Select(entry => new
            {
                communityId = entry.Key.ToString(),
                steamId = SteamIds.SteamId(entry.Key),
                name = entry.Value
            }));
    }
}
