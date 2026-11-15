using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.AntiCheat)]
[Route("admin/anti-cheat")]
public class AntiCheatAdminController(ISettingsService settingsService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Admin = "active";
        ViewBag.AntiCheatAdmin = "active";

        return View(await settingsService.GetAsync<AntiCheatSettings>(cancellationToken));
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

    private static string? Clean(string? address)
    {
        var trimmed = address?.Trim();

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps ? trimmed : null;
    }
}
