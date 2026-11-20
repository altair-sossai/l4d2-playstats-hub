using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[AutoValidateAntiforgeryToken]
[Route("setup")]
public class SetupController(
    ISetupService setupService,
    IAdministratorService administratorService,
    IAdminViewService adminViewService,
    IGameServerService gameServerService,
    ISettingsService settingsService,
    ISkinCatalogService skinCatalogService,
    ICurrentUser currentUser,
    IConfiguration configuration,
    IWebHostEnvironment environment) : Controller
{
    private const string TokenStep = "token";
    private const string SignInStep = "signin";
    private const string SiteStep = "site";
    private const string ModulesStep = "modules";
    private const string ServerStep = "server";
    private const string FinishStep = "finish";

    private bool DevelopmentSignIn => environment.IsDevelopment() && configuration.GetValue<bool>(AppConsts.DevelopmentSignInSetting);

    [HttpGet("")]
    public async Task<IActionResult> Index(string? step, string? token, int? server, CancellationToken cancellationToken)
    {
        if (!await setupService.IsPendingAsync(cancellationToken))
            return Redirect("/");

        var administrators = await administratorService.ListAsync(cancellationToken);

        if (administrators.Count == 0)
            return View(new SetupViewModel { Step = TokenStep, Token = token, DevelopmentSignIn = DevelopmentSignIn });

        if (!await currentUser.IsAdministratorAsync())
            return View(new SetupViewModel { Step = SignInStep, DevelopmentSignIn = DevelopmentSignIn });

        var current = step is ModulesStep or ServerStep or FinishStep ? step : SiteStep;

        var model = new SetupViewModel
        {
            Step = current,
            Settings = await adminViewService.SettingsAsync(cancellationToken),
            Modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken),
            Server = current == ServerStep ? await ServerAsync(server, cancellationToken) : null
        };

        return View(model);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start(string? token, long? developmentCommunityId, CancellationToken cancellationToken)
    {
        if (!await setupService.IsValidTokenAsync(token, cancellationToken))
            return View("Index", new SetupViewModel { Step = TokenStep, Token = token, Error = "The setup code is not valid.", DevelopmentSignIn = DevelopmentSignIn });

        Response.Cookies.Append(AppConsts.SetupCookie, token!.Trim(), new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(AppConsts.SetupCookieInMinutes)
        });

        if (DevelopmentSignIn && developmentCommunityId != null)
            return Redirect($"/dev/sign-in/{developmentCommunityId}?returnUrl=/setup/claim");

        return Challenge(new AuthenticationProperties { RedirectUri = "/setup/claim", IsPersistent = true });
    }

    [HttpGet("claim")]
    public async Task<IActionResult> Claim(CancellationToken cancellationToken)
    {
        var communityId = currentUser.CommunityId;
        var token = Request.Cookies[AppConsts.SetupCookie];

        if (communityId != null)
            await setupService.ClaimAsync(token, communityId.Value, currentUser.Name, cancellationToken);

        Response.Cookies.Delete(AppConsts.SetupCookie);

        return Redirect("/setup");
    }

    [HttpPost("site")]
    [RequestSizeLimit(AppConsts.SettingsFormMaxBytes)]
    public async Task<IActionResult> Site(SettingsViewModel model, CancellationToken cancellationToken)
    {
        if (!await currentUser.IsAdministratorAsync())
            return Forbid();

        var current = await adminViewService.SettingsAsync(cancellationToken);

        current.Site.Name = model.Site.Name;
        current.Site.TimeZone = model.Site.TimeZone;
        current.Site.DefaultCulture = model.Site.DefaultCulture;
        current.Site.PublicUrl = model.Site.PublicUrl;
        current.Site.SteamApiKey = string.IsNullOrWhiteSpace(model.Site.SteamApiKey) ? null : model.Site.SteamApiKey.Trim();
        current.Season.LengthInMonths = model.Season.LengthInMonths;
        current.Logo = model.Logo;

        var problems = await adminViewService.SaveAsync(current, cancellationToken);

        if (problems.Count == 0)
            return Redirect($"/setup?step={ModulesStep}");

        TempData[AppConsts.SettingsProblemsTempData] = problems.ToArray();

        return Redirect(AppConsts.SetupWizardUrl);
    }

    [HttpPost("modules")]
    public async Task<IActionResult> Modules(ModuleSettings modules, CancellationToken cancellationToken)
    {
        if (!await currentUser.IsAdministratorAsync())
            return Forbid();

        await settingsService.SaveAsync(modules, cancellationToken);
        await skinCatalogService.SeedAsync(cancellationToken);

        return Redirect($"/setup?step={ServerStep}");
    }

    [HttpPost("server")]
    public async Task<IActionResult> Server(ServerForm form, CancellationToken cancellationToken)
    {
        if (!await currentUser.IsAdministratorAsync())
            return Forbid();

        if (string.IsNullOrWhiteSpace(form.Name))
            return Redirect($"/setup?step={ServerStep}");

        var credentials = await gameServerService.CreateAsync(form.Name, form.Address, cancellationToken);
        TempData["Token"] = credentials.Token;

        return Redirect($"/setup?step={ServerStep}&server={credentials.Server.Id}");
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish(CancellationToken cancellationToken)
    {
        if (!await currentUser.IsAdministratorAsync())
            return Forbid();

        await setupService.CompleteAsync(cancellationToken);

        return Redirect("/admin");
    }

    private async Task<ServerViewModel?> ServerAsync(int? id, CancellationToken cancellationToken)
    {
        var server = id == null
            ? (await gameServerService.ListAsync(cancellationToken)).FirstOrDefault()
            : await gameServerService.FindAsync(id.Value, cancellationToken);

        if (server == null)
            return null;

        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return new ServerViewModel
        {
            Status = await adminViewService.StatusAsync(server, cancellationToken),
            Token = TempData["Token"] as string,
            SiteUrl = adminViewService.SiteUrl(Request, site),
            Wizard = true
        };
    }
}