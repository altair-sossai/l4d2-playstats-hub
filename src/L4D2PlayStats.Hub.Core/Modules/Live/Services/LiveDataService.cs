using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class LiveDataService(ILiveServerStore store, IGameServerPresence presence) : ILiveDataService
{
    public void UpdateConfiguration(GameServer server, LiveConfiguration? configuration)
    {
        var state = Received(server);

        state.Update(configuration);
    }

    public void UpdateRound(GameServer server, LiveRound? round)
    {
        var state = Received(server);

        state.Update(round);
    }

    public void UpdateScoreboard(GameServer server, LiveScoreboard? scoreboard)
    {
        var state = Received(server);

        state.Update(scoreboard);
    }

    public void UpdatePlayers(GameServer server, PlayersCommand? command)
    {
        var state = Received(server);

        state.Update(command ?? new PlayersCommand());
    }

    public void AddMessage(GameServer server, ChatMessageCommand? command)
    {
        var state = Received(server);

        if (command != null)
            state.AddMessage(command);
    }

    public void AddEvent(GameServer server, GameEvent? gameEvent)
    {
        var state = Received(server);

        if (gameEvent != null)
            state.AddEvent(gameEvent);
    }

    public LiveSnapshot Snapshot(GameServer server)
    {
        var state = store.For(server.Id);

        return state.Snapshot();
    }

    public IReadOnlyList<FeedItem> FeedAfter(GameServer server, long sequence)
    {
        var state = store.For(server.Id);

        return state.FeedAfter(sequence);
    }

    public IReadOnlyList<SiteChatMessage> SiteMessagesAfter(GameServer server, long ticks)
    {
        var state = store.For(server.Id);

        return state.SiteMessagesAfter(ticks);
    }

    private LiveServerState Received(GameServer server)
    {
        presence.LiveDataReceived(server.Id);

        return store.For(server.Id);
    }
}
