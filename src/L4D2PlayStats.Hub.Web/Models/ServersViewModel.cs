namespace L4D2PlayStats.Hub.Web.Models;

public class ServersViewModel
{
    public PagedListModel<ServerStatusViewModel> Servers { get; init; } = null!;
    public string ReturnUrl { get; init; } = AppConsts.AdminServersUrl;
}
