using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.AntiCheat)]
[Route("admin/anti-cheat")]
public class AntiCheatAdminController(ISettingsService settingsService, IAntiCheatAccessService accessService, ILiveViewService liveViewService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? list, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        ViewBag.Admin = "active";
        ViewBag.AntiCheatAdmin = "active";

        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);
        var access = await accessService.GetAsync(cancellationToken);
        var shownList = AntiCheatAccessNames.TryParseList(list, out var parsed) ? parsed : access.Mode.ListOf();
        var term = q?.Trim();

        var filtered = access[shownList]
            .Select(player => new AntiCheatAccessPlayerResult(player))
            .Where(player => string.IsNullOrEmpty(term)
                             || player.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || player.CommunityId.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || player.SteamId.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var timeZone = await liveViewService.TimeZoneAsync(cancellationToken);
        var model = new AntiCheatAdminModel(settings, access, shownList, new PagedListModel<AntiCheatAccessPlayerResult>(filtered, page, AppConsts.AntiCheatAdminPageSize), q, timeZone);

        return View(model);
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(string? serviceUrl, string? downloadUrl, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);

        settings.ServiceUrl = Clean(serviceUrl);
        settings.DownloadUrl = Clean(downloadUrl);

        await settingsService.SaveAsync(settings, cancellationToken);

        TempData["Saved"] = true;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet(AppConsts.AntiCheatAdminLegacyUrl)]
    public IActionResult Legacy()
    {
        return RedirectPermanent($"{AppConsts.AntiCheatAdminUrl}{Request.QueryString}");
    }

    private static string? Clean(string? address)
    {
        var trimmed = address?.Trim();

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? trimmed : null;
    }
}