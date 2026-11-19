using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Settings.Enums;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Services;

public class SkinCatalogService(
    HubDbContext dbContext,
    HubState hubState,
    IClock clock,
    ISettingsService settingsService,
    IHttpClientFactory httpClientFactory,
    ILogger<SkinCatalogService> logger) : ISkinCatalogService
{
    public async Task<SkinCatalogImportResult> ImportAsync(string? json, CancellationToken cancellationToken = default)
    {
        var entries = SkinCatalogReader.Read(json);
        if (entries == null)
            return SkinCatalogImportResult.Failed(SkinCatalogImportStatus.InvalidFile);

        if (entries.Count == 0)
            return SkinCatalogImportResult.Failed(SkinCatalogImportStatus.EmptyCatalog);

        await hubState.WriteLock.WaitAsync(cancellationToken);

        try
        {
            var result = await ApplyAsync(entries, cancellationToken);

            logger.LogInformation("Skin catalog imported: {Total} skins, {Added} added, {Updated} updated, {Removed} removed", result.Total, result.Added, result.Updated, result.Removed);

            return result;
        }
        finally
        {
            hubState.WriteLock.Release();
        }
    }

    public async Task<SkinCatalogImportResult> ImportFromUrlAsync(CancellationToken cancellationToken = default)
    {
        var settings = await settingsService.GetAsync<SkinShopSettings>(cancellationToken);
        var url = settings.CatalogUrl?.Trim();

        if (!SkinAssets.IsHttpUrl(url))
            return SkinCatalogImportResult.Failed(SkinCatalogImportStatus.UrlNotConfigured);

        var json = await DownloadAsync(url!, cancellationToken);

        return json == null ? SkinCatalogImportResult.Failed(SkinCatalogImportStatus.DownloadFailed) : await ImportAsync(json, cancellationToken);
    }

    public Task<SkinCatalogImportResult> ImportBundledAsync(CancellationToken cancellationToken = default)
    {
        return ImportAsync(SkinBundledCatalog.Json, cancellationToken);
    }

    public async Task<SkinCatalogImportResult?> SeedAsync(CancellationToken cancellationToken = default)
    {
        var modules = await settingsService.GetAsync<ModuleSettings>(cancellationToken);

        if (!modules.IsEnabled(HubModule.SkinShop) || await dbContext.Set<Skin>().AnyAsync(cancellationToken))
            return null;

        return await ImportBundledAsync(cancellationToken);
    }

    private async Task<string?> DownloadAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            using var client = httpClientFactory.CreateClient(AppConsts.SkinCatalogHttpClient);

            client.Timeout = TimeSpan.FromSeconds(AppConsts.SkinCatalogTimeoutInSeconds);
            client.MaxResponseContentBufferSize = AppConsts.SkinCatalogMaxBytes;

            using var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Could not download the skin catalog: HTTP {Status}", (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Could not download the skin catalog");

            return null;
        }
    }

    private async Task<SkinCatalogImportResult> ApplyAsync(List<SkinCatalogEntry> entries, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var skins = await dbContext.Set<Skin>().Include(skin => skin.Texts).ToDictionaryAsync(skin => skin.Id, cancellationToken);
        var authorNames = await dbContext.Set<SkinAuthor>().AsNoTracking().Where(author => author.Name != null).ToDictionaryAsync(author => author.CommunityId, author => author.Name!, cancellationToken);
        var listed = entries.Select(entry => entry.Id!).ToHashSet();
        var added = new HashSet<string>();

        foreach (var entry in entries)
        {
            if (!skins.TryGetValue(entry.Id!, out var skin))
            {
                skin = new Skin { Id = entry.Id!, ImportedAtUtc = now, UpdatedAtUtc = now };
                dbContext.Add(skin);
                added.Add(skin.Id);
            }

            Apply(skin, entry);

            if (skin.AuthorCommunityId != null && authorNames.TryGetValue(skin.AuthorCommunityId.Value, out var authorName))
                skin.AuthorName = authorName;
        }

        var removed = skins.Values.Where(skin => skin.Available && !listed.Contains(skin.Id)).ToList();

        foreach (var skin in removed)
            skin.Available = false;

        dbContext.ChangeTracker.DetectChanges();

        var changed = dbContext.ChangeTracker.Entries<Skin>()
            .Where(entry => entry.State == EntityState.Modified)
            .Select(entry => entry.Entity.Id)
            .Concat(dbContext.ChangeTracker.Entries<SkinText>().Where(entry => entry.State != EntityState.Unchanged).Select(entry => entry.Entity.SkinId))
            .Where(id => !added.Contains(id))
            .ToHashSet();

        foreach (var id in changed)
        {
            var skin = skins[id];
            skin.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new SkinCatalogImportResult
        {
            Status = SkinCatalogImportStatus.Success,
            Added = added.Count,
            Updated = changed.Count(listed.Contains),
            Removed = removed.Count,
            Total = entries.Count
        };
    }

    private void Apply(Skin skin, SkinCatalogEntry entry)
    {
        skin.Weapon = entry.Weapon!;
        skin.Rarity = entry.Rarity!.Value;
        skin.DefaultPrice = entry.Price!.Value;
        skin.WorkshopId = Clean(entry.WorkshopId, AppConsts.SkinWorkshopIdMaxLength);
        skin.AuthorName = Clean(entry.Author?.Name, AppConsts.SkinNameMaxLength);
        skin.AuthorCommunityId = long.TryParse(entry.Author?.CommunityId, out var communityId) ? communityId : null;
        skin.OriginalTitle = Clean(entry.OriginalTitle, AppConsts.SkinTitleMaxLength);
        skin.OriginalDescription = Clean(entry.OriginalDescription, int.MaxValue);
        skin.Shots = entry.Shots;
        skin.Available = true;

        var titles = Texts(entry.Title);
        var descriptions = Texts(entry.Description);
        var languages = titles.Keys.Union(descriptions.Keys).ToHashSet();

        foreach (var text in skin.Texts.Where(text => !languages.Contains(text.Language)).ToList())
        {
            skin.Texts.Remove(text);
            dbContext.Remove(text);
        }

        foreach (var language in languages)
        {
            var text = skin.Texts.FirstOrDefault(t => t.Language == language);

            if (text == null)
            {
                text = new SkinText { SkinId = skin.Id, Language = language };
                skin.Texts.Add(text);
            }

            text.Title = Clean(titles.GetValueOrDefault(language), AppConsts.SkinTitleMaxLength);
            text.Description = Clean(descriptions.GetValueOrDefault(language), int.MaxValue);
        }
    }

    private static Dictionary<string, string> Texts(Dictionary<string, string?>? texts)
    {
        var result = new Dictionary<string, string>();

        foreach (var (key, value) in texts ?? [])
        {
            var language = key.Trim().ToLowerInvariant();

            if (language.Length == 0 || language.Length > AppConsts.SkinLanguageMaxLength || string.IsNullOrWhiteSpace(value))
                continue;

            result[language] = value;
        }

        return result;
    }

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
