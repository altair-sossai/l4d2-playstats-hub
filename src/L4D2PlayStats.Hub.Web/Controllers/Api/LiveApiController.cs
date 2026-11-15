using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Web.Infrastructure;
using L4D2PlayStats.Hub.Web.Models.Live;
using L4D2PlayStats.Hub.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace L4D2PlayStats.Hub.Web.Controllers.Api;

[ApiController]
[Route("api/servers")]
[RequireModule(HubModule.LiveServer)]
public class LiveApiController(ILiveViewService liveViewService, ILiveDataService liveDataService) : ControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> Servers(CancellationToken cancellationToken)
    {
        var list = await liveViewService.ListAsync(cancellationToken);

        return Ok(list.Servers.Select(Summary));
    }

    [HttpGet("{publicId}/game-info")]
    public async Task<IActionResult> GameInfo(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        var card = await liveViewService.CardAsync(server, cancellationToken);
        var snapshot = card.Snapshot;

        return Ok(new
        {
            server = Summary(card),
            snapshot.Configuration,
            snapshot.Round,
            snapshot.Scoreboard,
            snapshot.Survivors,
            snapshot.Infecteds,
            snapshot.Spectators,
            snapshot.AnyPlayerConnected,
            snapshot.Messages,
            snapshot.Events
        });
    }

    [HttpGet("{publicId}/game-info/configuration")]
    public async Task<IActionResult> Configuration(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);

        return server == null ? NotFound() : Ok(liveDataService.Snapshot(server).Configuration);
    }

    [HttpGet("{publicId}/game-info/round")]
    public async Task<IActionResult> Round(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);

        return server == null ? NotFound() : Ok(liveDataService.Snapshot(server).Round);
    }

    [HttpGet("{publicId}/game-info/scoreboard")]
    public async Task<IActionResult> Scoreboard(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);

        return server == null ? NotFound() : Ok(liveDataService.Snapshot(server).Scoreboard);
    }

    [HttpGet("{publicId}/game-info/players")]
    public async Task<IActionResult> Players(string publicId, CancellationToken cancellationToken)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);
        if (server == null)
            return NotFound();

        var snapshot = liveDataService.Snapshot(server);

        return Ok(new { snapshot.Survivors, snapshot.Infecteds, snapshot.Spectators });
    }

    [HttpGet("{publicId}/game-info/messages")]
    public async Task<IActionResult> Messages(string publicId, [FromQuery] long after = 0, CancellationToken cancellationToken = default)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);

        return server == null ? NotFound() : Ok(liveDataService.Snapshot(server).Messages.Where(message => message.Ticks > after));
    }

    [HttpGet("{publicId}/game-info/events")]
    public async Task<IActionResult> Events(string publicId, [FromQuery] long after = 0, CancellationToken cancellationToken = default)
    {
        var server = await liveViewService.FindServerAsync(publicId, cancellationToken);

        return server == null ? NotFound() : Ok(liveDataService.Snapshot(server).Events.Where(gameEvent => gameEvent.Ticks > after));
    }

    private static object Summary(ServerCardViewModel card)
    {
        return new
        {
            publicId = card.Server.PublicId,
            name = card.Server.Name,
            address = card.Server.Address,
            state = card.Status.State,
            map = card.Map,
            campaign = card.Campaign,
            configuration = card.Snapshot.Configuration?.Name,
            teamSize = card.Snapshot.Configuration?.TeamSize,
            survivorScore = card.Snapshot.Scoreboard?.SurvivorScore,
            infectedScore = card.Snapshot.Scoreboard?.InfectedScore,
            players = card.Snapshot.PlayerCount
        };
    }
}
