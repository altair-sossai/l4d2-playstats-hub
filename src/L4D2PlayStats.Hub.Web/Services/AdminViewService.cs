using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Services;

public class AdminViewService(HubDbContext dbContext, IGameServerPresence presence, ISettingsService settingsService) : IAdminViewService
{
    public async Task<ServerStatusViewModel> StatusAsync(GameServer server, CancellationToken cancellationToken = default)
    {
        return new ServerStatusViewModel
        {
            Server = server,
            Status = presence.GetStatus(server),
            Modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken),
            Files = await dbContext.StatisticsFiles.CountAsync(f => f.GameServerId == server.Id, cancellationToken)
        };
    }

    public async Task<SettingsViewModel> SettingsAsync(CancellationToken cancellationToken = default)
    {
        return new SettingsViewModel
        {
            Site = await settingsService.GetAsync<SiteSettings>(cancellationToken),
            Season = await settingsService.GetAsync<SeasonSettings>(cancellationToken),
            Ranking = await settingsService.GetAsync<RankingSettings>(cancellationToken),
            TimeZones = [.. TimeZoneInfo.GetSystemTimeZones().Select(zone => zone.Id).OrderBy(id => id)]
        };
    }

    public async Task SaveAsync(SettingsViewModel model, CancellationToken cancellationToken = default)
    {
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        site.Name = string.IsNullOrWhiteSpace(model.Site.Name) ? site.Name : model.Site.Name.Trim();
        site.PublicUrl = string.IsNullOrWhiteSpace(model.Site.PublicUrl) ? null : model.Site.PublicUrl.Trim().TrimEnd('/');
        site.SteamApiKey = string.IsNullOrWhiteSpace(model.Site.SteamApiKey) ? null : model.Site.SteamApiKey.Trim();

        if (!string.IsNullOrWhiteSpace(model.Site.TimeZone) && TimeZoneInfo.TryFindSystemTimeZoneById(model.Site.TimeZone.Trim(), out _))
            site.TimeZone = model.Site.TimeZone.Trim();

        if (AppConsts.SupportedCultures.Contains(model.Site.DefaultCulture))
            site.DefaultCulture = model.Site.DefaultCulture;

        var season = new SeasonSettings { LengthInMonths = SeasonCalendar.NormalizeLength(model.Season.LengthInMonths) };

        var ranking = model.Ranking;
        ranking.MinimumMatches = Math.Max(0, ranking.MinimumMatches);

        await settingsService.SaveAsync(site, cancellationToken);
        await settingsService.SaveAsync(season, cancellationToken);
        await settingsService.SaveAsync(ranking, cancellationToken);
    }

    public string SiteUrl(HttpRequest request, SiteSettings site)
    {
        return string.IsNullOrEmpty(site.PublicUrl) ? $"{request.Scheme}://{request.Host}" : site.PublicUrl;
    }
}
