using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipService(HubDbContext dbContext, IClock clock, IChampionshipLogService log) : IChampionshipService
{
    public async Task<Page<ChampionshipSummary>> PageAsync(int page, int size, CancellationToken cancellationToken = default)
    {
        var ids = await dbContext.Championships.AsNoTracking().OrderByDescending(championship => championship.CreatedAtUtc).ThenByDescending(championship => championship.Id).Select(championship => championship.Id).ToPageAsync(page, size, cancellationToken);
        var summaries = (await Summaries(dbContext.Championships.AsNoTracking().Where(championship => ids.Items.Contains(championship.Id)), cancellationToken)).ToDictionary(summary => summary.Id);

        return ids.Map(id => summaries[id]);
    }

    public async Task<List<ChampionshipSummary>> ListAsync(bool publicOnly, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Championships.AsNoTracking();

        if (publicOnly)
            query = query.Where(championship => championship.Status != ChampionshipStatus.Draft);

        return (await Summaries(query, cancellationToken))
            .OrderBy(summary => summary.Status is ChampionshipStatus.Finished or ChampionshipStatus.Cancelled ? 1 : 0)
            .ThenByDescending(summary => summary.StartsOn ?? summary.CreatedAtUtc)
            .ThenByDescending(summary => summary.Id)
            .ToList();
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
                    championship.CreatedAtUtc);
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
        championship.Description = Text(form.Description, AppConsts.ChampionshipDescriptionMaxLength);
        championship.StreamUrl = Url(form.StreamUrl);
        championship.VideosUrl = Url(form.VideosUrl);
        championship.GameConfiguration = Text(form.GameConfiguration, AppConsts.ChampionshipConfigurationMaxLength);
        championship.PlayersPerGame = Math.Clamp(form.PlayersPerGame, 1, AppConsts.ChampionshipMaxPlayersPerGame);
        championship.RosterSize = Math.Clamp(form.RosterSize, championship.PlayersPerGame, AppConsts.ChampionshipMaxRoster);
        championship.MaxParticipants = form.MaxParticipants is > 0 ? Math.Min(form.MaxParticipants.Value, AppConsts.ChampionshipMaxTeams * AppConsts.ChampionshipMaxRoster) : null;
        championship.Substitutes = form.Substitutes;
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

        championship.Rules = Text(english, AppConsts.ChampionshipRulesMaxLength);
        championship.RulesPortuguese = Text(portuguese, AppConsts.ChampionshipRulesMaxLength);
        championship.RulesSpanish = Text(spanish, AppConsts.ChampionshipRulesMaxLength);
        championship.UpdatedAtUtc = clock.UtcNow;

        log.Add(id, updatedBy, "Saved the rules");
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetStatusAsync(int id, ChampionshipStatus status, string? reason, long? updatedBy, CancellationToken cancellationToken = default)
    {
        var championship = await dbContext.Championships.FirstOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw NotFound();

        if (!Enum.IsDefined(status))
            throw new ChampionshipException("Choose a valid status.");

        championship.Status = status;
        championship.CancelReason = status == ChampionshipStatus.Cancelled ? Text(reason, AppConsts.ChampionshipReasonMaxLength) : null;
        championship.UpdatedAtUtc = clock.UtcNow;

        log.Add(id, updatedBy, "Changed the status to {0}", status);
        await dbContext.SaveChangesAsync(cancellationToken);
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
        var trimmed = name?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            throw new ChampionshipException("Enter the name of the championship.");

        return trimmed.Length > AppConsts.ChampionshipNameMaxLength ? trimmed[..AppConsts.ChampionshipNameMaxLength] : trimmed;
    }

    private static string? Text(string? value, int maxLength)
    {
        var trimmed = value?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return null;

        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }

    private static string? Url(string? value)
    {
        var trimmed = Text(value, AppConsts.ChampionshipUrlMaxLength);

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
