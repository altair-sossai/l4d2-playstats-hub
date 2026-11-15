using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public record RemoteCommandResult(RemoteCommandStatus Status, RemoteCommandLog? Log = null)
{
    public bool Queued => Status == RemoteCommandStatus.Queued;
}
