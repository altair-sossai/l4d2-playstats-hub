using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Authorize(Policy = AppConsts.AdministratorPolicy)]
[AutoValidateAntiforgeryToken]
public abstract class AdminControllerBase : Controller
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.Admin = AppConsts.ActiveMenu;

        base.OnActionExecuting(context);
    }

    protected void Saved()
    {
        TempData[AppConsts.SavedTempData] = true;
    }

    protected void Error(string message)
    {
        TempData[AppConsts.ErrorTempData] = message;
    }

    protected bool IsLocal(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl);
    }

    protected IActionResult Back(string? returnUrl, string fallback)
    {
        return IsLocal(returnUrl) ? LocalRedirect(returnUrl!) : Redirect(fallback);
    }

    protected bool TryParseSteamId(string? value, out long communityId)
    {
        var parsed = SteamIdParser.Parse(value);

        communityId = parsed ?? 0;

        if (parsed == null)
            Error(AppConsts.InvalidSteamIdMessage);

        return parsed != null;
    }

    protected async Task<bool> ValidAsync<T>(T form, CancellationToken cancellationToken)
    {
        var error = await HttpContext.RequestServices.GetRequiredService<IValidationService>().ErrorAsync(form, cancellationToken);

        if (error != null)
            Error(error);

        return error == null;
    }

    protected IActionResult Legacy(string url)
    {
        return RedirectPermanent($"{url}{Request.QueryString}");
    }
}
