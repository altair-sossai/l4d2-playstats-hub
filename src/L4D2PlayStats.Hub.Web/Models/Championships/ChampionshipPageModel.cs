using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Web.Models.Championships;

public class ChampionshipPageModel
{
    public required Championship Championship { get; init; }
    public required Dictionary<long, PlayerProfile> Profiles { get; init; }
    public required Dictionary<int, ChampionshipCampaignOption> Campaigns { get; init; }
    public required TimeZoneInfo TimeZone { get; init; }
    public required string Tab { get; init; }
    public long? CurrentUser { get; init; }
    public bool Administrator { get; init; }
    public ChampionshipSeries? Series { get; init; }
    public List<ChampionshipPlayerStatistics> Statistics { get; init; } = [];
    public Dictionary<long, string> Names { get; set; } = [];

    public ChampionshipPodium Podium => ChampionshipPodiums.Of(Championship);

    public List<ChampionshipStage> Stages => Championship.Stages.OrderBy(stage => stage.Order).ToList();

    public List<ChampionshipTeam> Teams => Championship.Teams.OrderBy(team => team.Seed).ThenBy(team => team.Name).ToList();

    public IEnumerable<ChampionshipSeries> AllSeries => Stages.SelectMany(stage => stage.Series);

    public bool HasStandings => Stages.Any(stage => !stage.IsKnockout && stage.Generated);

    public bool HasBracket => Stages.Any(stage => stage.IsKnockout && stage.Generated);

    public bool HasDraft => Championship.DraftPicks.Count > 0 || Championship.Teams.Any(team => team.DraftPosition != null);

    public bool HasGames => Stages.Any(stage => stage.Generated);

    public ChampionshipTeam? Team(int? teamId)
    {
        return teamId == null ? null : Championship.Teams.FirstOrDefault(team => team.Id == teamId);
    }

    public string TeamName(int? teamId)
    {
        return Team(teamId)?.Name ?? "?";
    }

    public string PlayerName(long communityId)
    {
        var profile = Profiles.GetValueOrDefault(communityId);

        return profile?.DisplayName ?? profile?.Name ?? Names.GetValueOrDefault(communityId) ?? communityId.ToString();
    }

    public int? Hours(long communityId)
    {
        var minutes = Profiles.GetValueOrDefault(communityId)?.Left4Dead2Minutes;

        return minutes == null ? null : minutes / 60;
    }

    public string? CampaignName(int? campaignId)
    {
        return campaignId == null ? null : Campaigns.GetValueOrDefault(campaignId.Value)?.Name;
    }

    public ChampionshipStage StageOf(ChampionshipSeries series)
    {
        return Championship.Stages.First(stage => stage.Id == series.StageId);
    }

    public int Rounds(ChampionshipStage stage)
    {
        return stage.Series.Count == 0 ? 0 : stage.Series.Max(series => series.Round);
    }

    public string? GroupName(ChampionshipSeries series)
    {
        return series.GroupId == null ? null : StageOf(series).Groups.FirstOrDefault(group => group.Id == series.GroupId)?.Name;
    }

    public SeriesProgress Progress(ChampionshipSeries series)
    {
        return SeriesCalculator.Evaluate(series, StageOf(series).IsKnockout);
    }

    public ChampionshipSeriesStatus SeriesStatus(ChampionshipSeries series)
    {
        if (series.Bye)
            return ChampionshipSeriesStatus.Bye;

        if (!series.HasTeams)
            return ChampionshipSeriesStatus.Waiting;

        if (series.Finished)
            return ChampionshipSeriesStatus.Finished;

        return series.Games.Any(game => game.IsPlayed || game.IsInProgress) ? ChampionshipSeriesStatus.InProgress : ChampionshipSeriesStatus.Pending;
    }

    public List<ChampionshipStanding> Standings(ChampionshipStage stage, ChampionshipGroup group)
    {
        return StandingsCalculator.Calculate(stage, group.Teams.ToDictionary(team => team.TeamId, team => team.Seed), stage.Series.Where(series => series.GroupId == group.Id), stage.Penalties);
    }

    public int Qualifiers(ChampionshipStage stage)
    {
        return ChampionshipStageService.Next(Championship, stage) == null ? 0 : stage.Qualifiers;
    }

    public List<ChampionshipSeries> Upcoming()
    {
        return AllSeries.Where(series => series is { Finished: false, ScheduledAtUtc: not null } && series.HasTeams).OrderBy(series => series.ScheduledAtUtc).Take(AppConsts.ChampionshipUpcomingSeries).ToList();
    }

    public List<ChampionshipSeries> Recent()
    {
        return AllSeries.Where(series => series is { Finished: true, Bye: false }).OrderByDescending(series => series.Games.Where(game => game.PlayedAtUtc != null).Select(game => game.PlayedAtUtc).DefaultIfEmpty().Max()).ThenByDescending(series => series.Id).Take(AppConsts.ChampionshipUpcomingSeries).ToList();
    }

    public ChampionshipTeam? MyTeam()
    {
        if (CurrentUser == null || !Championship.IsPublic || Championship.IsOver)
            return null;

        return Championship.Teams.FirstOrDefault(team => team.IsLeader(CurrentUser.Value));
    }

    public string? FlagUrl(int? teamId)
    {
        var team = Team(teamId);

        return team?.Flag == null ? null : ChampionshipFlags.Url(team);
    }

    public string? Rules(string culture)
    {
        var translated = culture.StartsWith(AppConsts.ChampionshipPortugueseCulture, StringComparison.OrdinalIgnoreCase) ? Championship.RulesPortuguese
            : culture.StartsWith(AppConsts.ChampionshipSpanishCulture, StringComparison.OrdinalIgnoreCase) ? Championship.RulesSpanish
            : null;

        return string.IsNullOrWhiteSpace(translated) ? Championship.Rules ?? Championship.RulesPortuguese ?? Championship.RulesSpanish : translated;
    }

    public List<ChampionshipCampaignOption> PickOptions(ChampionshipSeries series, ChampionshipGame game, int teamId)
    {
        return CampaignRules.ForPick(StageOf(series), series, game, teamId, Campaigns.Values, AllSeries.SelectMany(item => item.Games));
    }

    public List<ChampionshipCampaignOption> VetoOptions(ChampionshipSeries series, bool teamA)
    {
        return CampaignRules.ForVeto(StageOf(series), series, teamA, Campaigns.Values);
    }

    public string LogText(ChampionshipLog log, Func<string, object[], string> localize)
    {
        var values = (log.Values ?? string.Empty).Split(Core.AppConsts.ChampionshipLogValueSeparator, StringSplitOptions.None).Select(value => (object)(long.TryParse(value, out var id) && (Profiles.ContainsKey(id) || Names.ContainsKey(id)) ? PlayerName(id) : value)).ToArray();

        return localize(log.Text, log.Values == null ? [] : values);
    }
}
