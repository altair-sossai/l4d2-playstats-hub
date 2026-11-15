using L4D2PlayStats.Hub.Core.Modules.Live;
using L4D2PlayStats.Hub.Core.Modules.Live.Models;

namespace L4D2PlayStats.Hub.Web.Models.Live;

public class SiteChatFormViewModel
{
    public string PublicId { get; init; } = null!;
    public bool SignedIn { get; init; }
    public ChatBan? Ban { get; init; }
    public SiteChatResult? Result { get; init; }
    public string? Message { get; init; }
}
