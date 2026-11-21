using L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Patents.Models;
using L4D2PlayStats.Hub.Core.Modules.Patents.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Patents.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Patents.Services;

public class PatentService(ISettingsService settingsService) : IPatentService
{
    public async Task<PatentScale> ScaleAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<PatentSettings>(cancellationToken);

        var standard = settings.MaxExperience == AppConsts.PatentMaxExperience && settings.Curve.Equals(AppConsts.PatentCurve);

        return standard ? PatentCatalog.Default : PatentCatalog.Scale(settings.MaxExperience, settings.Curve);
    }
}
