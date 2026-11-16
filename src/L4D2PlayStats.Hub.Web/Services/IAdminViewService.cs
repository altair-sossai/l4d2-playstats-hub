using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Web.Models;

namespace L4D2PlayStats.Hub.Web.Services;

public interface IAdminViewService
{
    Task<ServerStatusViewModel> StatusAsync(GameServer server, CancellationToken cancellationToken = default);
    Task<SettingsViewModel> SettingsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> SaveAsync(SettingsViewModel model, CancellationToken cancellationToken = default);
    string SiteUrl(HttpRequest request, SiteSettings site);
}
