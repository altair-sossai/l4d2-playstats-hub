using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipService(HubDbContext dbContext, IClock clock, IChampionshipLogService log, IChampionshipNotificationService notify) : IChampionshipService
{
    public Task<Page<ChampionshipSummary>> PageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        return PageOfAsync(dbContext.Championships.AsNoTracking().OrderByDescending(championship => championship.CreatedAtUtc).ThenByDescending(championship => championship.Id), page, size, cancellationToken);
    }

    public Task<Page<ChampionshipSummary>> HistoryAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Championships.AsNoTracking()
            .Where(championship => championship.Status == ChampionshipStatus.Finished || championship.Status == ChampionshipStatus.Cancelled)
            .OrderByDescending(championship => championship.EndsOn ?? championship.StartsOn ?? championship.CreatedAtUtc)
            .ThenByDescending(championship => championship.Id);

        return PageOfAsync(query, page, size, cancellationToken);
    }

    public async Task<List<ChampionshipSummary>> ActiveAsync(CancellationToken cancellationToken = default)
    {
        var query = dbContext.Championships.AsNoTracking().Where(championship => championship.Status != ChampionshipStatus.Draft && championship.Status != ChampionshipStatus.Finished && championship.Status != ChampionshipStatus.Cancelled);

        return (await Summaries(query, cancellationToken))
            .OrderByDescending(summary => summary.StartsOn ?? summary.CreatedAtUtc)
            .ThenByDescending(summary => summary.Id)
            .ToList();
    }

    private async Task<Page<ChampionshipSummary>> PageOfAsync(IQueryable<Championship> ordered, int page, int size, CancellationToken cancellationToken)
    {
        var ids = await ordered.Select(championship => championship.Id).ToPageAsync(page, size, cancellationToken);
        var summaries = (await Summaries(dbContext.Championships.AsNoTracking().Where(championship => ids.Items.Contains(championship.Id)), cancellationToken)).ToDictionary(summary => summary.Id);

        return ids.Map(id => summaries[id]);
    }

    private static async Task<List<ChampionshipSummary>> Summaries(IQueryable<Championship> source, CancellationToken cancellationToken)
    {
        var query = source
            .Include(championship => championship.Teams)
            .Include(championship => championship.Participants)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Groups).ThenInclude(group => group.Teams)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Series).ThenInclude(series => series.Games)
            .Include(championship => championship.Stages).ThenInclude(stage => stage.Penalties)
            .AsSplitQuery();

        var championships = await query.ToListAsync(cancellationToken);

        return championships
            .Select(championship =>
            {
                var podium = championship.Status == ChampionshipStatus.Finished ? ChampionshipPodiums.Of(championship) : ChampionshipPodium.None;
                var names = championship.Teams.ToDictionary(team => team.Id, team => team.Name);

                return new ChampionshipSummary(championship.Id, championship.Name, championship.Slug, championship.Description, championship.Status, championship.StartsOn, championship.EndsOn,
                    championship.Teams.Count, championship.Participants.Count,
                    podium.Champion == null ? null : names.GetValueOrDefault(podium.Champion.Value),
                    podium.RunnerUp == null ? null : names.GetValueOrDefault(podium.RunnerUp.Value),
                    championship.CreatedAtUtc, championship.BannerImage, championship.LogoImage);
            })
            .ToList();
    }

    public async Task<Championship> CreateAsync(string name, IReadOnlyList<ChampionshipStageTemplate> stages, long? createdBy, CancellationToken cancellationToken = default)
    {
        var trimmed = Required(name);
        var now = clock.UtcNow;
        var championship = new Championship
        {
            Name = trimmed,
            Slug = await UniqueSlugAsync(ChampionshipSlugs.From(trimmed), null, cancellationToken),
            Status = ChampionshipStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        for (var index = 0; index < stages.Count; index++)
            championship.Stages.Add(Stage(stages[index], index + 1));

        dbContext.Championships.Add(championship);
        await dbContext.SaveChangesAsync(cancellationToken);

        log.Add(championship.Id, createdBy, "Created the championship {0}", championship.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        return championship;
    }

    public async Task UpdateAsync(int id, ChampionshipSettingsForm form, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw NotFound();
        var slug = string.IsNullOrWhiteSpace(form.Slug) ? ChampionshipSlugs.From(form.Name) : ChampionshipSlugs.From(form.Slug);

        if (form.EndsOn != null && form.StartsOn != null && form.EndsOn < form.StartsOn)
            throw new ChampionshipException("The end date must be after the start date.");

        championship.Name = Required(form.Name);
        championship.Slug = await UniqueSlugAsync(slug, id, cancellationToken);
        championship.Description = Text.Clip(form.Description, AppConsts.ChampionshipDescriptionMaxLength);
        championship.StreamUrl = Url(form.StreamUrl);
        championship.VideosUrl = Url(form.VideosUrl);
        championship.GameConfiguration = Text.Clip(form.GameConfiguration, AppConsts.ChampionshipConfigurationMaxLength);
        championship.PlayersPerGame = Math.Clamp(form.PlayersPerGame, 1, AppConsts.ChampionshipMaxPlayersPerGame);
        championship.RosterSize = Math.Clamp(form.RosterSize, championship.PlayersPerGame, AppConsts.ChampionshipMaxRoster);
        championship.MaxParticipants = form.MaxParticipants is > 0 ? Math.Min(form.MaxParticipants.Value, AppConsts.ChampionshipMaxTeams * AppConsts.ChampionshipMaxRoster) : null;
        championship.Substitutes = form.Substitutes;
        championship.JoinRequests = form.JoinRequests;
        championship.DraftMethod = Enum.IsDefined(form.DraftMethod) ? form.DraftMethod : ChampionshipDraftMethod.Manual;
        championship.AutoLinkMatches = form.AutoLinkMatches;
        championship.AutoLinkHours = Math.Clamp(form.AutoLinkHours, 1, AppConsts.ChampionshipMaxAutoLinkHours);
        championship.AutoCloseMinutes = Math.Clamp(form.AutoCloseMinutes, AppConsts.ChampionshipMinAutoCloseMinutes, AppConsts.ChampionshipMaxAutoCloseMinutes);
        championship.StartsOn = form.StartsOn?.Date;
        championship.EndsOn = form.EndsOn?.Date;
        championship.UpdatedAtUtc = clock.UtcNow;

        log.Add(id, updatedBy, "Saved the settings");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRulesAsync(int id, string? english, string? portuguese, string? spanish, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw NotFound();

        championship.Rules = Text.Clip(english, AppConsts.ChampionshipRulesMaxLength);
        championship.RulesPortuguese = Text.Clip(portuguese, AppConsts.ChampionshipRulesMaxLength);
        championship.RulesSpanish = Text.Clip(spanish, AppConsts.ChampionshipRulesMaxLength);
        championship.UpdatedAtUtc = clock.UtcNow;

        log.Add(id, updatedBy, "Saved the rules");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStatusAsync(int id, ChampionshipStatus status, string? reason, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.Include(item => item.Participants).Include(item => item.Teams).ThenInclude(team => team.Members).AsSplitQuery().FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw NotFound();

        if (!Enum.IsDefined(status))
            throw new ChampionshipException("Choose a valid status.");

        var previous = championship.Status;

        championship.Status = status;
        championship.CancelReason = status == ChampionshipStatus.Cancelled ? Text.Clip(reason, AppConsts.ChampionshipReasonMaxLength) : null;
        championship.UpdatedAtUtc = clock.UtcNow;

        log.Add(id, updatedBy, "Changed the status to {0}", status);
        notify.StatusChanged(championship, previous);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChampionshipImageSet?> ImagesAsync(int id, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return championship?.Images;
    }

    public Task<bool> IsPublicAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Championships.AnyAsync(championship => championship.Id == id && championship.Status != ChampionshipStatus.Draft, cancellationToken);
    }

    public async Task<ChampionshipImageSet> SetImagesAsync(int id, ChampionshipImageSet images, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw NotFound();
        var previous = championship.Images;
        var gallery = images.Gallery.Where(image => !string.IsNullOrWhiteSpace(image)).Select(image => image.Trim()).Distinct().ToList();

        if (gallery.Count > AppConsts.ChampionshipGalleryMaxImages)
            throw new ChampionshipException("The gallery is full. Remove an image before adding another.");

        if (new[] { images.Banner, images.Logo }.OfType<string>().Concat(gallery).Any(image => image.Length > AppConsts.ChampionshipImageMaxLength || image.Contains(AppConsts.ChampionshipGallerySeparator)))
            throw new ChampionshipException("The images must be PNG, JPG, WEBP or GIF files.");

        championship.BannerImage = Image(images.Banner);
        championship.LogoImage = Image(images.Logo);
        championship.GalleryImages = ChampionshipGallery.Format(gallery);

        if (championship.BannerImage != previous.Banner)
            log.Add(id, updatedBy, previous.Banner == null ? "Added the banner" : championship.BannerImage == null ? "Removed the banner" : "Changed the banner");

        if (championship.LogoImage != previous.Logo)
            log.Add(id, updatedBy, previous.Logo == null ? "Added the logo" : championship.LogoImage == null ? "Removed the logo" : "Changed the logo");

        var added = gallery.Except(previous.Gallery).Count();
        var removed = previous.Gallery.Except(gallery).Count();

        if (added > 0)
            log.Add(id, updatedBy, "Added images to the gallery ({0})", added);

        if (removed > 0)
            log.Add(id, updatedBy, "Removed images from the gallery ({0})", removed);

        if (championship.BannerImage == previous.Banner && championship.LogoImage == previous.Logo && gallery.SequenceEqual(previous.Gallery))
            return previous;

        championship.UpdatedAtUtc = clock.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return previous;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (championship == null)
            return false;

        var stageIds = await dbContext.ChampionshipStages.Where(stage => stage.ChampionshipId == id).Select(stage => stage.Id).ToListAsync(cancellationToken);
        var teamIds = await dbContext.ChampionshipTeams.Where(team => team.ChampionshipId == id).Select(team => team.Id).ToListAsync(cancellationToken);

        dbContext.ChampionshipPenalties.RemoveRange(dbContext.ChampionshipPenalties.Where(penalty => stageIds.Contains(penalty.StageId)));
        dbContext.ChampionshipGroupTeams.RemoveRange(dbContext.ChampionshipGroupTeams.Where(team => teamIds.Contains(team.TeamId)));
        dbContext.ChampionshipDraftPicks.RemoveRange(dbContext.ChampionshipDraftPicks.Where(pick => pick.ChampionshipId == id));
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Championships.Remove(championship);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public static ChampionshipStage Stage(ChampionshipStageTemplate template, int order)
    {
        return new ChampionshipStage
        {
            Order = order,
            Name = template.Name.Trim(),
            Type = template.Type,
            BestOf = template.BestOf,
            FinalBestOf = template.BestOf,
            GroupCount = template.GroupCount,
            Qualifiers = template.Qualifiers,
            CampaignRule = template.CampaignRule,
            Seeding = template.Seeding,
            OncePerTeam = template.CampaignRule == ChampionshipCampaignRule.Picks,
            MinimumMaps = template.CampaignRule == ChampionshipCampaignRule.RoundDraw ? AppConsts.ChampionshipDrawMinimumMaps : 0,
            Tiebreakers = ChampionshipTiebreakers.Format(ChampionshipTiebreakers.Default)
        };
    }

    private async Task<string> UniqueSlugAsync(string slug, int? id, CancellationToken cancellationToken)
    {
        var candidate = slug;

        for (var attempt = 2; attempt < AppConsts.ChampionshipSlugMaxAttempts; attempt++)
        {
            var value = candidate;

            if (!await dbContext.Championships.AnyAsync(championship => championship.Slug == value && championship.Id != id, cancellationToken))
                return candidate;

            var suffix = $"-{attempt}";

            candidate = (slug.Length + suffix.Length > AppConsts.ChampionshipSlugMaxLength ? slug[..(AppConsts.ChampionshipSlugMaxLength - suffix.Length)] : slug) + suffix;
        }

        throw new ChampionshipException("Choose another address for the championship.");
    }

    private static string Required(string? name)
    {
        return Text.Clip(name, AppConsts.ChampionshipNameMaxLength) ?? throw new ChampionshipException("Enter the name of the championship.");
    }

    private static string? Image(string? value)
    {
        var trimmed = value?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }


    private static string? Url(string? value)
    {
        var trimmed = Text.Clip(value, AppConsts.ChampionshipUrlMaxLength);

        if (trimmed == null)
            return null;

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ChampionshipException("Links must start with http:// or https://.");

        return trimmed;
    }

    private static ChampionshipException NotFound()
    {
        return new ChampionshipException("The championship was not found.");
    }
}
