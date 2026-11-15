using L4D2PlayStats.Hub.Core.Modules.Live.Models;
using L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;
using L4D2PlayStats.Hub.Core.Modules.Servers;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Services;

public interface ILiveDataService
{
    void UpdateConfiguration(GameServer server, LiveConfiguration? configuration);
    void UpdateRound(GameServer server, LiveRound? round);
    void UpdateScoreboard(GameServer server, LiveScoreboard? scoreboard);
    void UpdatePlayers(GameServer server, PlayersCommand? command);
    void AddMessage(GameServer server, ChatMessageCommand? command);
    void AddEvent(GameServer server, GameEvent? gameEvent);
    LiveSnapshot Snapshot(GameServer server);
    IReadOnlyList<FeedItem> FeedAfter(GameServer server, long sequence);
    IReadOnlyList<SiteChatMessage> SiteMessagesAfter(GameServer server, long ticks);
}
