using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.AntiCheat)]
public class AntiCheatController(
    ISettingsService settingsService,
    IAntiCheatVersionService antiCheatVersionService,
    IAntiCheatAccessService accessService,
    ICurrentUser currentUser) : Controller
{
    [HttpGet(AppConsts.AntiCheatUrl)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.AntiCheat = "active";

        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);
        var latest = await antiCheatVersionService.GetLatestAsync(cancellationToken);

        return View(new AntiCheatModel(latest, settings.DownloadUrl) { CurrentUserRequired = await CurrentUserRequiredAsync(cancellationToken) });
    }

    [HttpGet("/anticheat")]
    public IActionResult Legacy()
    {
        return RedirectPermanent(AppConsts.AntiCheatUrl);
    }

    private async Task<bool?> CurrentUserRequiredAsync(CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CommunityId == null)
            return null;

        var access = await accessService.GetAsync(cancellationToken);

        return access.IsRequired(currentUser.CommunityId.Value);
    }
}
