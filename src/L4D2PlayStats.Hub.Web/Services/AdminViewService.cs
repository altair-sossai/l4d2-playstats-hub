using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Seasons.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Servers.Services;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Infrastructure.Branding;
using L4D2PlayStats.Hub.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Web.Services;

public class AdminViewService(HubDbContext dbContext, IGameServerPresence presence, ISettingsService settingsService, BrandingStore brandingStore) : IAdminViewService
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

    public async Task<IReadOnlyList<string>> SaveAsync(SettingsViewModel model, CancellationToken cancellationToken = default)
    {
        var problems = new List<string>();
        var site = await settingsService.GetAsync<SiteSettings>(cancellationToken);

        site.Name = string.IsNullOrWhiteSpace(model.Site.Name) ? site.Name : model.Site.Name.Trim();
        site.PublicUrl = string.IsNullOrWhiteSpace(model.Site.PublicUrl) ? null : model.Site.PublicUrl.Trim().TrimEnd('/');
        site.SteamApiKey = string.IsNullOrWhiteSpace(model.Site.SteamApiKey) ? null : model.Site.SteamApiKey.Trim();
        site.FooterLinks = FooterLinks.Normalize(model.Site.FooterLinks);
        site.MetaKeywords = MetaKeywords(model.Site.MetaKeywords);

        if (FooterLinks.HasInvalidLines(model.Site.FooterLinks))
            problems.Add(AppConsts.FooterLinksIgnoredMessage);

        var analyticsId = model.Site.GoogleAnalyticsId?.Trim();

        if (string.IsNullOrEmpty(analyticsId))
            site.GoogleAnalyticsId = null;
        else if (Regex.IsMatch(analyticsId, AppConsts.GoogleAnalyticsIdPattern))
            site.GoogleAnalyticsId = analyticsId;
        else
            problems.Add(AppConsts.GoogleAnalyticsIdInvalidMessage);

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

        var logoProblem = await SaveLogoAsync(model, cancellationToken);

        if (logoProblem != null)
            problems.Add(logoProblem);

        return problems;
    }

    public string SiteUrl(HttpRequest request, SiteSettings site)
    {
        return string.IsNullOrEmpty(site.PublicUrl) ? $"{request.Scheme}://{request.Host}" : site.PublicUrl;
    }

    private async Task<string?> SaveLogoAsync(SettingsViewModel model, CancellationToken cancellationToken)
    {
        var logo = model.Logo;

        if (logo == null || logo.Length == 0)
        {
            if (model.RemoveLogo)
                brandingStore.RemoveLogo();

            return null;
        }

        if (!BrandingStore.IsSupported(logo.FileName))
            return AppConsts.LogoUnsupportedMessage;

        if (logo.Length > AppConsts.LogoMaxBytes)
            return AppConsts.LogoTooLargeMessage;

        await using var content = logo.OpenReadStream();

        var header = new byte[AppConsts.ImageSignatureLength];
        var read = await content.ReadAtLeastAsync(header, header.Length, false, cancellationToken);

        if (!ImageSignature.IsImage(header.AsSpan(0, read)))
            return AppConsts.LogoUnsupportedMessage;

        content.Position = 0;

        await brandingStore.SaveLogoAsync(content, logo.FileName, cancellationToken);

        return null;
    }

    private static string? MetaKeywords(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var keywords = string.Join(", ", text.Split([',', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return keywords.Length > AppConsts.MetaKeywordsMaxLength ? keywords[..AppConsts.MetaKeywordsMaxLength] : keywords;
    }
}
