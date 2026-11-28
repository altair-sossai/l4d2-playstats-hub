using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

public abstract class HubControllerBase : Controller
{
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

    protected async Task<IActionResult> RunAsync<TException>(Func<Task> action, Func<IActionResult> next) where TException : Exception
    {
        try
        {
            await action();
            Saved();
        }
        catch (TException exception)
        {
            Error(exception.Message);
        }

        return next();
    }
}
