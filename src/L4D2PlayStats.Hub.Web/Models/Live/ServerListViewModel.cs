namespace L4D2PlayStats.Hub.Web.Models.Live;

public class ServerListViewModel
{
    public IReadOnlyList<ServerCardViewModel> Servers { get; init; } = [];
}
