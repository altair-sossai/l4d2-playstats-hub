using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Helpers;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[RequireModule(HubModule.AntiCheat)]
[Route("admin/anti-cheat")]
public class AntiCheatAdminController(ISettingsService settingsService, IAntiCheatAccessService accessService, ISiteClock siteClock) : AdminControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? list, string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);
        var access = await accessService.ModeAsync(cancellationToken);
        var shownList = AntiCheatAccessNames.TryParseList(list, out var parsed) ? parsed : access.Mode.ListOf();
        var players = await accessService.PageAsync(shownList, PlayerSearch.Of(q), page, AppConsts.AntiCheatAdminPageSize, cancellationToken);
        var listed = await accessService.IdsAsync(shownList, cancellationToken);
        var others = await accessService.IdsAsync(shownList.Other(), cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);
        var rows = new PagedListModel<AntiCheatAccessPlayerResult>(players.Map(player => new AntiCheatAccessPlayerResult(player)));

        return View(new AntiCheatAdminModel(settings, access, shownList, rows, q, timeZone, listed, others.Count));
    }

    [HttpPost("")]
    public async Task<IActionResult> Save(string? serviceUrl, string? downloadUrl, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync<AntiCheatSettings>(cancellationToken);

        settings.ServiceUrl = SafeUrl.Https(serviceUrl);
        settings.DownloadUrl = SafeUrl.Https(downloadUrl);

        await settingsService.SaveAsync(settings, cancellationToken);

        Saved();

        return Redirect(AdminTabs.Url(AppConsts.AntiCheatAdminUrl, AppConsts.AntiCheatPageTab));
    }

    [HttpGet(AppConsts.AntiCheatAdminLegacyUrl)]
    public IActionResult Legacy()
    {
        return Legacy(AppConsts.AntiCheatAdminUrl);
    }
}
