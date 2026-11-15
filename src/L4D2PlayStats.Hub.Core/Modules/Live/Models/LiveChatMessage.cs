using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class LiveChatMessage : FeedItem
{
    public LiveChatMessage(ChatMessageCommand command, DateTime when)
    {
        When = when;
        Public = command.Public;
        Team = command.Team;
        Player = new LivePlayer { CommunityId = command.CommunityId, Name = command.Name };
        IsAdmin = command.IsAdmin;
        Message = command.Message?.Trim();
    }

    public LiveChatMessage(SiteChatMessage message)
    {
        When = message.When;
        Team = LiveTeam.External;
        Player = new LivePlayer { CommunityId = message.CommunityId, Name = message.Name };
        Message = message.Text;
    }

    public bool Public { get; }
    public LiveTeam Team { get; }
    public LivePlayer Player { get; }
    public bool IsAdmin { get; }
    public string? Message { get; }
}
