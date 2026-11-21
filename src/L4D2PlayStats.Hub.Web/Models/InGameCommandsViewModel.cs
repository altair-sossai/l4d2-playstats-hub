using L4D2PlayStats.Hub.Web.Models.Commands;

namespace L4D2PlayStats.Hub.Web.Models;

public class InGameCommandsViewModel : InGameViewModel
{
    public IReadOnlyList<CommandSection> Sections { get; init; } = [];
}
