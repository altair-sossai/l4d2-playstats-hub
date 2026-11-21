using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Patents.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Patents.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
[RequireModule(HubModule.Ranking)]
[Route("admin/patents")]
public class PatentsAdminController(ISettingsService settingsService, IPatentService patentService) : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = "active";
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(new PatentsAdminViewModel { Scale = await patentService.ScaleAsync(cancellationToken) });
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(int maxExperience, string? curve, CancellationToken cancellationToken)
    {
        var settings = new PatentSettings { MaxExperience = maxExperience, Curve = ParseCurve(curve) };

        settings.Normalize();

        await settingsService.SaveAsync(settings, cancellationToken);
        TempData["Saved"] = true;

        return Redirect(AppConsts.AdminPatentsUrl);
    }

    private static double ParseCurve(string? curve)
    {
        return double.TryParse(curve?.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : CoreConsts.PatentCurve;
    }
}
