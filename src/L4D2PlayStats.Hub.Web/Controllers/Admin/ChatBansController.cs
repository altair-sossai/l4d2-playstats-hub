using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Players.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Live;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Admin;

[RequireModule(HubModule.SiteChat)]
public class ChatBansController(IChatBanService chatBanService, ISiteClock siteClock, ISettingsService settingsService) : AdminControllerBase
{
    [HttpGet("/admin/chat-bans")]
    public async Task<IActionResult> Index(string? q, int page = 1, CancellationToken cancellationToken = default)
    {
        var bans = await chatBanService.PageAsync(PlayerSearch.Of(q), page, AppConsts.ChatBansPageSize, cancellationToken);
        var total = await chatBanService.CountAsync(cancellationToken);
        var timeZone = await siteClock.ZoneAsync(cancellationToken);
        var limits = await settingsService.GetAsync<SiteChatSettings>(cancellationToken);
        var rows = new PagedListModel<ChatBanResult>(bans.Map(ban => new ChatBanResult(ban)));

        return View(new ChatBansModel(rows, q, total, timeZone, limits));
    }

    [HttpPost("/admin/chat-bans/limits")]
    public async Task<IActionResult> Limits(SiteChatSettings limits, CancellationToken cancellationToken)
    {
        await settingsService.SaveAsync(limits, cancellationToken);
        TempData[AppConsts.LimitsSavedTempData] = true;

        return Redirect(AdminTabs.Url(AppConsts.ChatBansUrl, AppConsts.ChatBansLimitsTab));
    }

    [HttpGet("/chat-bans")]
    public IActionResult Legacy()
    {
        return Legacy(AppConsts.ChatBansUrl);
    }
}
