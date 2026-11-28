using L4D2PlayStats.Hub.Core.Modules.Patents.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using L4D2PlayStats.Hub.Web.Models.Ranking;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers;

[RequireModule(HubModule.Ranking)]
public class PatentsController(ISettingsService settingsService, IPatentService patentService) : Controller
{
    [HttpGet("/patents")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Patent = AppConsts.ActiveMenu;

        var config = await settingsService.GetAsync<RankingSettings>(cancellationToken);
        var season = await settingsService.GetAsync<SeasonSettings>(cancellationToken);

        return View(new PatentModel { Config = config, SeasonLengthInMonths = season.LengthInMonths, Patents = (await patentService.ScaleAsync(cancellationToken)).All });
    }
}
