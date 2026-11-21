using L4D2PlayStats.Hub.Core.Modules.Players.Extensions;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public class LiveDataService(ILiveServerStore store, IGameServerPresence presence, IForcedNameStore forcedNames) : ILiveDataService
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
        var players = command ?? new PlayersCommand();

        foreach (var player in (players.Survivors ?? []).Cast<LivePlayer?>().Concat(players.Infecteds ?? []).Concat(players.Spectators ?? []))
            Rename(player);

        state.Update(players);
    }

    public void AddMessage(GameServer server, ChatMessageCommand? command)
    {
        var state = Received(server);

        if (command == null)
            return;

        command.Name = forcedNames.Apply(command.CommunityId, command.Name);
        state.AddMessage(command);
    }

    public void AddEvent(GameServer server, GameEvent? gameEvent)
    {
        var state = Received(server);

        if (gameEvent == null)
            return;

        foreach (var property in gameEvent.GetType().GetProperties().Where(property => property.CanRead && typeof(LivePlayer).IsAssignableFrom(property.PropertyType)))
            Rename(property.GetValue(gameEvent) as LivePlayer);

        state.AddEvent(gameEvent);
    }

    private void Rename(LivePlayer? player)
    {
        if (player?.Id != null)
            player.Name = forcedNames.Find(player.Id.Value) ?? player.Name;
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
