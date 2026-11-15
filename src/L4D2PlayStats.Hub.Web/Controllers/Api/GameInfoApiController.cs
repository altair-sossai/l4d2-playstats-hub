using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/game-info")]
[RequireModule(HubModule.LiveServer)]
[GameServerAuthorize]
public class GameInfoApiController(
    ILiveDataService liveDataService,
    IPlayerConnectionService playerConnectionService,
    IRemoteCommandService remoteCommandService,
    IGameServerPresence presence) : ControllerBase
{
    [HttpPut("configuration")]
    public IActionResult Configuration([FromBody] LiveConfiguration? configuration)
    {
        liveDataService.UpdateConfiguration(HttpContext.GameServer(), configuration);

        return Ok();
    }

    [HttpPut("round")]
    public IActionResult Round([FromBody] LiveRound? round)
    {
        liveDataService.UpdateRound(HttpContext.GameServer(), round);

        return Ok();
    }

    [HttpPut("scoreboard")]
    public IActionResult Scoreboard([FromBody] LiveScoreboard? scoreboard)
    {
        liveDataService.UpdateScoreboard(HttpContext.GameServer(), scoreboard);

        return Ok();
    }

    [HttpPut("players")]
    public IActionResult Players([FromBody] PlayersCommand? command)
    {
        liveDataService.UpdatePlayers(HttpContext.GameServer(), command);

        return Ok();
    }

    [HttpPut("messages")]
    public IActionResult Message([FromBody] ChatMessageCommand? command)
    {
        liveDataService.AddMessage(HttpContext.GameServer(), command);

        return Ok();
    }

    [HttpPost("events")]
    public IActionResult Event([FromBody] GameEvent? gameEvent)
    {
        liveDataService.AddEvent(HttpContext.GameServer(), gameEvent);

        return Ok();
    }

    [HttpPost("player-connection-info")]
    public async Task<IActionResult> PlayerConnection([FromBody] PlayerConnectionCommand? command, CancellationToken cancellationToken)
    {
        var server = HttpContext.GameServer();

        presence.LiveDataReceived(server.Id);

        var related = await playerConnectionService.RegisterAsync(command, cancellationToken);

        return related == null ? BadRequest(new { error = "A valid communityId, ipAddress and name are required" }) : Ok(related);
    }

    [HttpGet("server-command/dequeue")]
    public async Task<IActionResult> Dequeue(CancellationToken cancellationToken)
    {
        var command = await remoteCommandService.DequeueAsync(HttpContext.GameServer(), cancellationToken);

        return command == null ? NoContent() : Ok(command);
    }
}
