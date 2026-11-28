using L4D2PlayStats.Hub.Core.Modules.Bans.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Models.Bans;
using L4D2PlayStats.Hub.Web.Services.Contracts;

namespace L4D2PlayStats.Hub.Web.Services;

public class BanViewService(ISettingsService settingsService, IBanService banService, IDisplayNameStore displayNames, ISiteClock siteClock) : IBanViewService
{
    public async Task<PlayerBanModel?> PlayerAsync(long communityId, CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        if (!modules.IsEnabled(HubModule.Bans))
            return null;

        var ban = await banService.ActiveAsync(communityId, cancellationToken);

        if (ban == null)
            return null;

        var name = displayNames.Apply(communityId, ban.Name ?? communityId.ToString());

        return new PlayerBanModel(ban, name, await siteClock.ZoneAsync(cancellationToken));
    }
}
