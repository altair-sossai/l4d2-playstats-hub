using L4D2PlayStats.Hub.Core.Modules.Live.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models;

public record SiteChatResult(SiteChatError? Error, string? Reason = null)
{
    public bool Success => Error == null;

    public static SiteChatResult Sent()
    {
        return new SiteChatResult(Error: null);
    }

    public static SiteChatResult Failed(SiteChatError error, string? reason = null)
    {
        return new SiteChatResult(error, reason);
    }
}
