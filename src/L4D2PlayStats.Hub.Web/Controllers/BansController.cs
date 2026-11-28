using L4D2PlayStats.Hub.Core.Modules.Bans.Enums;
using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Bans;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Bans)]
public class BansController(IBanService banService, ISiteClock siteClock) : Controller
{
    [HttpGet(AppConsts.BansUrl)]
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Bans = AppConsts.ActiveMenu;

        var bans = await banService.PageAsync(PlayerSearch.Of(q), BanListFilter.Active, page, AppConsts.BansPublicPageSize, cancellationToken);

        return View(new BansIndexModel(new PagedListModel<Core.Modules.Bans.Ban>(bans), q?.Trim(), await siteClock.ZoneAsync(cancellationToken)));
    }
}
