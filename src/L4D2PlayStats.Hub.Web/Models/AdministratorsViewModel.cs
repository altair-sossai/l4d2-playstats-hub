using L4D2PlayStats.Hub.Core.Modules.Administrators;

namespace L4D2PlayStats.Hub.Web.Models;

public class AdministratorsViewModel
{
    public PagedListModel<Administrator> Administrators { get; init; } = null!;
    public long? Current { get; init; }
    public string ReturnUrl { get; init; } = AppConsts.AdminAdministratorsUrl;
}
