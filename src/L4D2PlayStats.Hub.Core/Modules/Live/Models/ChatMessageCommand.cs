using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public class ChatMessageCommand
{
    public bool Public { get; set; }
    public LiveTeam Team { get; set; }
    public string? CommunityId { get; set; }
    public string? Name { get; set; }
    public bool IsAdmin { get; set; }
    public string? Message { get; set; }
}
