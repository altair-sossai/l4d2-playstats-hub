using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.SkinShop)]
public class SkinsController(ISkinViewService skinViewService) : Controller
{
    [HttpGet(AppConsts.SkinShopUrl)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Skins = "active";

        return View(await skinViewService.ShopAsync(cancellationToken));
    }
}