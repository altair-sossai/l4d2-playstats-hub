using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Campaigns;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Matches.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Admin;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[Route("admin")]
public class AdminController(
    HubDbContext dbContext,
    IAdminViewService adminViewService,
    IGameServerService gameServerService,
    IGameModeService gameModeService,
    ICampaignService campaignService,
    IAdministratorService administratorService,
    ISettingsService settingsService,
    IMatchBuildService matchBuildService,
    ISkinCatalogService skinCatalogService,
    ILiveSettings liveSettings,
    ICurrentUser currentUser) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var servers = new List<ServerStatusViewModel>();

        foreach (var server in await gameServerService.ListAsync(cancellationToken))
            servers.Add(await adminViewService.StatusAsync(server, cancellationToken));

        var ruleSet = await gameModeService.GetRuleSetAsync(cancellationToken);
        var configurations = await gameModeService.ListConfigurationsAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            Servers = servers,
            Files = await dbContext.StatisticsFiles.CountAsync(cancellationToken),
            Matches = await dbContext.Matches.CountAsync(cancellationToken),
            RankedMatches = await dbContext.Matches.CountAsync(m => m.CountsForRanking, cancellationToken),
            Players = await dbContext.PlayerProfiles.CountAsync(cancellationToken),
            Seasons = await dbContext.Seasons.CountAsync(cancellationToken),
            UnmatchedConfigurations = configurations.Count(c => ruleSet.Find(c.Name, c.TeamSize) == null)
        };

        return View(model);
    }

    [HttpGet("servers")]
    public async Task<IActionResult> Servers(int page = 1, CancellationToken cancellationToken = default)
    {
        var listed = await gameServerService.PageAsync(page, AppConsts.GameServersPageSize, cancellationToken);
        var servers = await listed.MapAsync(server => adminViewService.StatusAsync(server, cancellationToken));

        return View(new ServersViewModel
        {
            Servers = new PagedListModel<ServerStatusViewModel>(servers),
            ReturnUrl = AdminListUrls.TabPage(AppConsts.AdminServersUrl, AppConsts.ServersListTab, AppConsts.AdminPageQuery, servers.Number)
        });
    }

    [HttpPost("servers/live")]
    public async Task<IActionResult> LiveSettings(LiveServerSettings settings, CancellationToken cancellationToken)
    {
        await settingsService.SaveAsync(settings, cancellationToken);
        liveSettings.Set(settings);
        TempData[AppConsts.LiveSavedTempData] = true;

        return Redirect(AdminTabs.Url(AppConsts.AdminServersUrl, AppConsts.ServersLiveTab));
    }

    [HttpPost("servers")]
    public async Task<IActionResult> CreateServer(ServerForm form, CancellationToken cancellationToken)
    {
        if (!await ValidAsync(form, cancellationToken))
            return RedirectToAction(nameof(Servers));

        var credentials = await gameServerService.CreateAsync(form.Name!, form.Address, cancellationToken);
        await gameServerService.SetAlternativeAddressAsync(credentials.Server.Id, form.AlternativeAddress, cancellationToken);
        TempData[AppConsts.TokenTempData] = credentials.Token;

        return RedirectToAction(nameof(Server), new { id = credentials.Server.Id });
    }

    [HttpGet("servers/{id:int}")]
    public async Task<IActionResult> Server(int id, CancellationToken cancellationToken)
    {
        var server = await gameServerService.FindAsync(id, cancellationToken);
        if (server == null)
            return NotFound();

        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        var model = new ServerViewModel
        {
            Status = await adminViewService.StatusAsync(server, cancellationToken),
            Token = TempData[AppConsts.TokenTempData] as string,
            SiteUrl = adminViewService.SiteUrl(Request, site)
        };

        return View(model);
    }

    [HttpGet("servers/{id:int}/status")]
    [AllowDuringSetup]
    public async Task<IActionResult> ServerStatus(int id, CancellationToken cancellationToken)
    {
        var server = await gameServerService.FindAsync(id, cancellationToken);
        if (server == null)
            return NotFound();

        return PartialView("_ServerStatus", await adminViewService.StatusAsync(server, cancellationToken));
    }

    [HttpPost("servers/{id:int}")]
    public async Task<IActionResult> UpdateServer(int id, ServerForm form, CancellationToken cancellationToken)
    {
        if (await ValidAsync(form, cancellationToken))
        {
            await gameServerService.UpdateAsync(id, form.Name!, form.Address, form.Enabled, cancellationToken);
            await gameServerService.SetAlternativeAddressAsync(id, form.AlternativeAddress, cancellationToken);
        }

        return RedirectToAction(nameof(Server), new { id });
    }

    [HttpPost("servers/{id:int}/key")]
    public async Task<IActionResult> RegenerateKey(int id, CancellationToken cancellationToken)
    {
        var credentials = await gameServerService.RegenerateKeyAsync(id, cancellationToken);
        if (credentials == null)
            return NotFound();

        TempData[AppConsts.TokenTempData] = credentials.Token;

        return RedirectToAction(nameof(Server), new { id });
    }

    [HttpPost("servers/{id:int}/delete")]
    public async Task<IActionResult> DeleteServer(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (!await gameServerService.DeleteAsync(id, cancellationToken))
            Error(AppConsts.ServerInUseMessage);

        return Back(returnUrl, AppConsts.AdminServersUrl);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        return View(await adminViewService.SettingsAsync(cancellationToken));
    }

    [HttpPost("settings")]
    [RequestSizeLimit(AppConsts.SettingsFormMaxBytes)]
    public async Task<IActionResult> SaveSettings(SettingsViewModel model, string? tab, CancellationToken cancellationToken)
    {
        var problems = await adminViewService.SaveAsync(model, cancellationToken);

        Saved();

        if (problems.Count > 0)
            TempData[AppConsts.SettingsProblemsTempData] = problems.ToArray();

        return Redirect(AdminTabs.Url(AppConsts.AdminSettingsUrl, AdminTabs.Active(tab, AppConsts.SettingsSiteTab, AppConsts.SettingsAppearanceTab, AppConsts.SettingsSeasonsTab, AppConsts.SettingsRankingTab, AppConsts.SettingsPrivacyTab)));
    }

    [HttpGet("modules")]
    public async Task<IActionResult> Modules(CancellationToken cancellationToken)
    {
        return View(await settingsService.GetAsync<ModuleSettings>(cancellationToken));
    }

    [HttpPost("modules")]
    public async Task<IActionResult> SaveModules(ModuleSettings modules, CancellationToken cancellationToken)
    {
        await settingsService.SaveAsync(modules, cancellationToken);
        await skinCatalogService.SeedAsync(cancellationToken);
        Saved();

        return RedirectToAction(nameof(Modules));
    }

    [HttpGet("game-modes")]
    public async Task<IActionResult> GameModes(int page = 1, int seenPage = 1, CancellationToken cancellationToken = default)
    {
        var rules = await gameModeService.RulesPageAsync(page, AppConsts.GameModeRulesPageSize, cancellationToken);
        var configurations = await gameModeService.ConfigurationsPageAsync(seenPage, AppConsts.GameConfigurationsPageSize, cancellationToken);
        var ruleSet = await gameModeService.GetRuleSetAsync(cancellationToken);

        var model = new GameModesViewModel
        {
            Rules = new PagedListModel<GameModeRule>(rules),
            Configurations = new PagedListModel<SeenConfiguration>(configurations.Map(c => new SeenConfiguration(c, ruleSet.Find(c.Name, c.TeamSize)))),
            ReturnUrl = AdminListUrls.TabPage(AppConsts.AdminGameModesUrl, AppConsts.GameModesRulesTab, AppConsts.AdminPageQuery, rules.Number)
        };

        return View(model);
    }

    [HttpPost("game-modes")]
    public async Task<IActionResult> SaveGameMode(GameModeRule rule, string? returnUrl, CancellationToken cancellationToken)
    {
        if (await ValidAsync(rule, cancellationToken))
            await gameModeService.SaveRuleAsync(rule, cancellationToken);

        return Back(returnUrl, AppConsts.AdminGameModesUrl);
    }

    [HttpPost("game-modes/{id:int}/delete")]
    public async Task<IActionResult> DeleteGameMode(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        await gameModeService.DeleteRuleAsync(id, cancellationToken);

        return Back(returnUrl, AppConsts.AdminGameModesUrl);
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns(int page = 1, CancellationToken cancellationToken = default)
    {
        var campaigns = await campaignService.PageAsync(page, AppConsts.CampaignsPageSize, cancellationToken);

        return View(new CampaignsViewModel
        {
            Campaigns = new PagedListModel<Campaign>(campaigns),
            ReturnUrl = AdminListUrls.Page(AppConsts.AdminCampaignsUrl, campaigns.Number)
        });
    }

    [HttpPost("campaigns")]
    public async Task<IActionResult> SaveCampaign(CampaignForm form, string? returnUrl, CancellationToken cancellationToken)
    {
        if (await ValidAsync(form, cancellationToken))
            await campaignService.SaveAsync(form.Id, form.Name!, CampaignMaps.Parse(form.Maps), cancellationToken);

        return Back(returnUrl, AppConsts.AdminCampaignsUrl);
    }

    [HttpPost("campaigns/{id:int}/delete")]
    public async Task<IActionResult> DeleteCampaign(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        await campaignService.DeleteAsync(id, cancellationToken);

        return Back(returnUrl, AppConsts.AdminCampaignsUrl);
    }

    [HttpGet("administrators")]
    public async Task<IActionResult> Administrators(int page = 1, CancellationToken cancellationToken = default)
    {
        var administrators = await administratorService.PageAsync(page, AppConsts.AdministratorsPageSize, cancellationToken);

        return View(new AdministratorsViewModel
        {
            Administrators = new PagedListModel<Administrator>(administrators),
            Current = currentUser.CommunityId,
            ReturnUrl = AdminListUrls.Page(AppConsts.AdminAdministratorsUrl, administrators.Number)
        });
    }

    [HttpPost("administrators")]
    public async Task<IActionResult> AddAdministrator(AdministratorForm form, CancellationToken cancellationToken)
    {
        if (await ValidAsync(form, cancellationToken))
            await administratorService.AddAsync(SteamIdParser.Parse(form.CommunityId)!.Value, form.Name, cancellationToken);

        return Back(form.ReturnUrl, AppConsts.AdminAdministratorsUrl);
    }

    [HttpPost("administrators/{communityId:long}/delete")]
    public async Task<IActionResult> RemoveAdministrator(long communityId, string? returnUrl, CancellationToken cancellationToken)
    {
        if (communityId == currentUser.CommunityId || !await administratorService.RemoveAsync(communityId, cancellationToken))
            Error(AppConsts.LastAdministratorMessage);

        return Back(returnUrl, AppConsts.AdminAdministratorsUrl);
    }

    [HttpPost("reprocess")]
    public async Task<IActionResult> Reprocess(CancellationToken cancellationToken)
    {
        TempData[AppConsts.ReprocessedTempData] = await matchBuildService.RebuildAllAsync(cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}
