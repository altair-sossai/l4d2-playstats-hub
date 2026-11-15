using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[RequireModule(HubModule.SkinShop)]
public class SkinsApiController(ISkinShopService skinShopService) : ControllerBase
{
    [HttpGet("api/skins")]
    public async Task<IActionResult> Skins(CancellationToken cancellationToken)
    {
        var skins = await skinShopService.GetPurchasedSkinsAsync(cancellationToken);

        return Ok(skins.Select(skin => new { id = skin.Id, weapon = skin.Weapon }));
    }
}
