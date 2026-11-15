using L4D2PlayStats.Hub.Core.Modules.Administrators;

namespace L4D2PlayStats.Hub.Web.Models;

public class AdministratorsViewModel
{
    public IReadOnlyList<Administrator> Administrators { get; init; } = [];
    public long? Current { get; init; }
}
