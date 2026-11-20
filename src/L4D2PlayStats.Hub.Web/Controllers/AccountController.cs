using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class AccountController : Controller
{
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        return Challenge(new AuthenticationProperties { RedirectUri = Local(returnUrl), IsPersistent = true });
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Redirect(Local(returnUrl));
    }

    [HttpGet("language")]
    public IActionResult Language(string culture, string? returnUrl = null)
    {
        if (AppConsts.SupportedCultures.Contains(culture))
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });
        }

        return Redirect(Local(returnUrl));
    }

    private string Local(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
    }
}
