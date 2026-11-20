using L4D2PlayStats.Hub.Web.Models.Api;
using L4D2PlayStats.Hub.Core.Modules.Servers.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Web.Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/servers")]
public class ServersApiController(IGameServerService gameServerService, ISettingsService settingsService) : ControllerBase
{
    [HttpPost("heartbeat")]
    [GameServerAuthorize]
    public async Task<IActionResult> Heartbeat([FromBody] HeartbeatCommand? command, CancellationToken cancellationToken)
    {
        var server = HttpContext.GameServer();

        var signal = new GameServerSignal
        {
            Hostname = command?.Hostname,
            Map = command?.Map,
            Configuration = command?.Configuration,
            MaxPlayers = command?.MaxPlayers is > 0 ? command.MaxPlayers : null,
            SourceModVersion = command?.SourceModVersion,
            StatisticsWriting = command?.StatisticsWriting,
            Plugins = command?.Plugins ?? []
        };

        await gameServerService.SignalAsync(server, signal, cancellationToken);

        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        return Ok(new
        {
            server = server.Name,
            site = site.Name,
            modules = Enum.GetValues<HubModule>().Where(modules.IsEnabled).Select(module => module.ToString())
        });
    }
}
