using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.AntiCheat)]
public class AntiCheatController(ISettingsService settingsService, IAntiCheatVersionService antiCheatVersionService) : Controller
{
    [HttpGet(AppConsts.AntiCheatUrl)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.AntiCheat = "active";

        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);
        var latest = await antiCheatVersionService.GetLatestAsync(cancellationToken);

        return View(new AntiCheatModel(latest, settings.DownloadUrl));
    }

    [HttpGet("/anticheat")]
    public IActionResult Legacy()
    {
        return RedirectPermanent(AppConsts.AntiCheatUrl);
    }
}
