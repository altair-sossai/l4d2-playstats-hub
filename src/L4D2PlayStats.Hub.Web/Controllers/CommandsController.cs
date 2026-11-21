using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Commands.Contracts;
using L4D2PlayStats.Hub.Web.Models.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace L4D2PlayStats.Hub.Web.Controllers;

public class CommandsController(ISettingsService settingsService, ICommandCatalog commandCatalog) : Controller
{
    [HttpGet(AppConsts.CommandsUrl)]
    [OutputCache(PolicyName = AppConsts.PageCachePolicy)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.Commands = "active";

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        return View(new CommandsViewModel { Sections = commandCatalog.Sections(CultureInfo.CurrentUICulture, modules) });
    }
}
