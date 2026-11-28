using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Services;

public class ChampionshipStageService(
    HubDbContext dbContext,
    IClock clock,
    IChampionshipLoadService loader,
    IChampionshipProgressService progress,
    IChampionshipRandomService random,
    IChampionshipLogService log) : IChampionshipStageService
{
    public async Task<ChampionshipStage> AddAsync(int championshipId, string name, ChampionshipStageType type, long? by, CancellationToken cancellationToken = default)
    {
        var championship = await loader.LoadAsync(championshipId, true, cancellationToken) ?? throw new ChampionshipException("The championship was not found.");

        if (championship.Stages.Count >= AppConsts.ChampionshipMaxStages)
            throw new ChampionshipException("The championship already has the maximum number of stages.");

        if (championship.Stages.OrderBy(stage => stage.Order).LastOrDefault() is { IsKnockout: true })
            throw new ChampionshipException("A knockout stage must be the last one.");

        var rule = type == ChampionshipStageType.Knockout ? ChampionshipCampaignRule.Picks : ChampionshipCampaignRule.RoundDraw;
        var seeding = championship.Stages.Count == 0 ? ChampionshipSeeding.Seeds : ChampionshipSeeding.Standings;
        var stage = ChampionshipService.Stage(new ChampionshipStageTemplate(string.IsNullOrWhiteSpace(name) ? type.ToString() : name, type, 1, 0, type == ChampionshipStageType.Knockout ? 3 : 1, rule, seeding), championship.Stages.Count == 0 ? 1 : championship.Stages.Max(item => item.Order) + 1);

        championship.Stages.Add(stage);
        log.Add(championshipId, by, "Added the stage {0}", stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);

        return stage;
    }

    public async Task UpdateAsync(int stageId, ChampionshipStageForm form, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);
        var name = form.Name.Trim();

        if (name.Length == 0)
            throw new ChampionshipException("Enter the name of the stage.");

        stage.Name = name.Length > AppConsts.ChampionshipStageNameMaxLength ? name[..AppConsts.ChampionshipStageNameMaxLength] : name;
        stage.PointsForWin = Math.Clamp(form.PointsForWin, 0, AppConsts.ChampionshipMaxPoints);
        stage.PointsForDraw = Math.Clamp(form.PointsForDraw, 0, AppConsts.ChampionshipMaxPoints);
        stage.PointsForLoss = Math.Clamp(form.PointsForLoss, 0, AppConsts.ChampionshipMaxPoints);
        stage.Tiebreakers = ChampionshipTiebreakers.Format(form.Tiebreakers.Where(Enum.IsDefined));
        stage.Veto = form.Veto;
        stage.OncePerTeam = form.OncePerTeam;
        stage.MinimumMaps = Math.Clamp(form.MinimumMaps, 0, AppConsts.ChampionshipMaxMinimumMaps);

        var next = Next(championship, stage);

        if (next is not { Generated: true })
            stage.Qualifiers = Math.Clamp(form.Qualifiers, 0, AppConsts.ChampionshipMaxTeams);

        if (!stage.Generated)
        {
            var isLast = next == null;

            if (form.Type == ChampionshipStageType.Knockout && !isLast)
                throw new ChampionshipException("A knockout stage must be the last one.");

            stage.Type = Enum.IsDefined(form.Type) ? form.Type : ChampionshipStageType.RoundRobin;
            stage.BestOf = Math.Clamp(form.BestOf, 1, AppConsts.ChampionshipMaxBestOf);
            stage.FinalBestOf = stage.IsKnockout ? Math.Clamp(form.FinalBestOf, 1, AppConsts.ChampionshipMaxBestOf) : stage.BestOf;
            stage.Legs = Math.Clamp(form.Legs, 1, AppConsts.ChampionshipMaxLegs);
            stage.GroupCount = stage.IsKnockout ? 1 : Math.Clamp(form.GroupCount, 1, AppConsts.ChampionshipMaxGroups);
            stage.Seeding = Previous(championship, stage) == null && form.Seeding == ChampionshipSeeding.Standings ? ChampionshipSeeding.Seeds : Enum.IsDefined(form.Seeding) ? form.Seeding : ChampionshipSeeding.Seeds;
            stage.ThirdPlace = stage.IsKnockout && form.ThirdPlace;
            stage.CampaignRule = Enum.IsDefined(form.CampaignRule) ? form.CampaignRule : ChampionshipCampaignRule.Manual;
        }

        log.Add(championship.Id, by, "Saved the stage {0}", stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int stageId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        if (stage.Generated)
            throw new ChampionshipException("Reset the stage before deleting it.");

        championship.Stages.Remove(stage);

        var order = 1;

        foreach (var item in championship.Stages.OrderBy(item => item.Order))
            item.Order = order++;

        log.Add(championship.Id, by, "Deleted the stage {0}", stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<int>> EntrantsAsync(int stageId, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        return Entrants(championship, stage, false);
    }

    public async Task GenerateAsync(int stageId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        if (stage.Generated)
            throw new ChampionshipException("The stage was already built. Reset it to build it again.");

        var previous = Previous(championship, stage);

        if (previous is { Generated: false })
            throw new ChampionshipException("Build the previous stage first.");

        if (previous != null && previous.Series.Any(series => !series.Finished))
            throw new ChampionshipException("The previous stage is not finished yet.");

        var teams = Entrants(championship, stage, true);

        if (teams.Count < 2)
            throw new ChampionshipException("At least two teams are needed to build this stage.");

        var campaigns = await loader.CampaignsAsync(cancellationToken);

        if (stage.IsKnockout)
            await BuildKnockoutAsync(championship, stage, teams, campaigns, cancellationToken);
        else
            await BuildRoundRobinAsync(stage, teams, campaigns, cancellationToken);

        stage.Generated = true;

        if (championship.Status < ChampionshipStatus.Running)
            championship.Status = ChampionshipStatus.Running;

        championship.UpdatedAtUtc = clock.UtcNow;
        log.Add(championship.Id, by, "Built the stage {0} with {1} teams", stage.Name, teams.Count);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(int stageId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        if (Next(championship, stage) is { Generated: true })
            throw new ChampionshipException("Reset the next stage first.");

        foreach (var series in stage.Series)
            series.NextSeriesId = series.LoserNextSeriesId = null;

        await dbContext.SaveChangesAsync(cancellationToken);

        stage.Series.Clear();
        stage.Groups.Clear();
        stage.Generated = false;
        log.Add(championship.Id, by, "Reset the stage {0}", stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DrawCampaignsAsync(int stageId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        if (stage.CampaignRule != ChampionshipCampaignRule.RoundDraw)
            throw new ChampionshipException("This stage does not draw one campaign per round.");

        DrawRounds(stage, await loader.CampaignsAsync(cancellationToken));
        log.Add(championship.Id, by, "Drew the campaigns of the stage {0}", stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCampaignsAsync(int stageId, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        foreach (var game in stage.Series.SelectMany(series => series.Games).Where(game => game is { IsPlayed: false, IsInProgress: false }))
        {
            game.CampaignId = null;
            game.CampaignSource = null;
            game.PickedByTeamId = null;
        }

        log.Add(championship.Id, by, "Cleared the campaigns of the stage {0}", stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetRoundDeadlineAsync(int stageId, int round, DateTime? deadlineUtc, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);

        foreach (var series in stage.Series.Where(series => series.Round == round))
            series.DeadlineUtc = deadlineUtc;

        log.Add(championship.Id, by, "Set the deadline of round {0} of the stage {1}", round, stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddPenaltyAsync(int stageId, int teamId, int points, string? reason, long? by, CancellationToken cancellationToken = default)
    {
        var (championship, stage) = await Load(stageId, cancellationToken);
        var team = championship.Teams.FirstOrDefault(item => item.Id == teamId) ?? throw new ChampionshipException("The team was not found.");

        if (points == 0 || Math.Abs(points) > AppConsts.ChampionshipMaxPenalty)
            throw new ChampionshipException("Enter the points of the penalty.");

        var trimmed = reason?.Trim();

        stage.Penalties.Add(new ChampionshipPenalty
        {
            TeamId = teamId,
            Points = points,
            Reason = string.IsNullOrEmpty(trimmed) ? null : trimmed.Length > AppConsts.ChampionshipReasonMaxLength ? trimmed[..AppConsts.ChampionshipReasonMaxLength] : trimmed,
            CreatedByCommunityId = by,
            CreatedAtUtc = clock.UtcNow
        });

        log.Add(championship.Id, by, "Penalized {0} with {1} points in the stage {2}", team.Name, points, stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePenaltyAsync(int penaltyId, long? by, CancellationToken cancellationToken = default)
    {
        var penalty = await dbContext.ChampionshipPenalties.FindAsync([penaltyId], cancellationToken) ?? throw new ChampionshipException("The penalty was not found.");
        var (championship, stage) = await Load(penalty.StageId, cancellationToken);
        var tracked = stage.Penalties.First(item => item.Id == penaltyId);

        stage.Penalties.Remove(tracked);
        log.Add(championship.Id, by, "Removed a penalty of {0} points in the stage {1}", tracked.Points, stage.Name);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static ChampionshipStage? Previous(Championship championship, ChampionshipStage stage)
    {
        return championship.Stages.Where(item => item.Order < stage.Order).MaxBy(item => item.Order);
    }

    public static ChampionshipStage? Next(Championship championship, ChampionshipStage stage)
    {
        return championship.Stages.Where(item => item.Order > stage.Order).MinBy(item => item.Order);
    }

    public static List<int> Qualified(Championship championship, ChampionshipStage stage)
    {
        var groups = stage.Groups.OrderBy(group => group.Order).ToList();
        var standings = groups.Select(group => StandingsCalculator.Calculate(stage, group.Teams.ToDictionary(team => team.TeamId, team => team.Seed), stage.Series.Where(series => series.GroupId == group.Id), stage.Penalties)).ToList();
        var deepest = standings.Count == 0 ? 0 : standings.Max(list => list.Count);
        var take = stage.Qualifiers <= 0 ? deepest : Math.Min(stage.Qualifiers, deepest);
        var qualified = new List<int>();

        for (var rank = 0; rank < take; rank++)
            foreach (var list in standings)
                if (rank < list.Count)
                    qualified.Add(list[rank].TeamId);

        return qualified;
    }

    private List<int> Entrants(Championship championship, ChampionshipStage stage, bool shuffle)
    {
        var previous = Previous(championship, stage);
        List<int> teams;

        if (previous == null)
            teams = championship.Teams.OrderBy(team => team.Seed).ThenBy(team => team.Id).Select(team => team.Id).ToList();
        else if (previous.IsKnockout)
            throw new ChampionshipException("A knockout stage must be the last one.");
        else if (!previous.Generated)
            return [];
        else
            teams = Qualified(championship, previous);

        return shuffle && stage.Seeding == ChampionshipSeeding.Random ? ChampionshipShuffle.Shuffle(teams, random.Next) : teams;
    }

    private async Task BuildRoundRobinAsync(ChampionshipStage stage, List<int> teams, IReadOnlyList<ChampionshipCampaignOption> campaigns, CancellationToken cancellationToken)
    {
        var split = GroupSplitter.Split(teams, stage.GroupCount);
        var seeds = teams.Select((team, index) => (team, index)).ToDictionary(pair => pair.team, pair => pair.index + 1);
        var groups = new List<ChampionshipGroup>();

        for (var index = 0; index < split.Count; index++)
        {
            var group = new ChampionshipGroup { Name = GroupSplitter.Name(index), Order = index + 1 };

            foreach (var team in split[index])
                group.Teams.Add(new ChampionshipGroupTeam { TeamId = team, Seed = seeds[team] });

            stage.Groups.Add(group);
            groups.Add(group);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var positions = new Dictionary<int, int>();

        for (var index = 0; index < split.Count; index++)
            foreach (var pairing in RoundRobinScheduler.Rounds(split[index], stage.Legs))
            {
                positions[pairing.Round] = positions.GetValueOrDefault(pairing.Round) + 1;
                stage.Series.Add(NewSeries(stage.BestOf, pairing.Round, positions[pairing.Round], groups[index].Id, pairing.TeamA, pairing.TeamB));
            }

        if (stage.CampaignRule == ChampionshipCampaignRule.RoundDraw)
            DrawRounds(stage, campaigns);
    }

    private async Task BuildKnockoutAsync(Championship championship, ChampionshipStage stage, List<int> teams, IReadOnlyList<ChampionshipCampaignOption> campaigns, CancellationToken cancellationToken)
    {
        var rounds = ChampionshipBracket.Rounds(teams.Count);
        var size = ChampionshipBracket.Size(teams.Count);
        var created = new Dictionary<(int Round, int Position), ChampionshipSeries>();

        foreach (var slot in ChampionshipBracket.FirstRound(teams))
        {
            var bye = slot.TeamA == null || slot.TeamB == null;
            var series = NewSeries(rounds == 1 ? stage.FinalBestOf : stage.BestOf, 1, slot.Position, null, slot.TeamA ?? slot.TeamB, bye ? null : slot.TeamB, !bye);

            series.Bye = bye;
            created[(1, slot.Position)] = series;
            stage.Series.Add(series);
        }

        for (var round = 2; round <= rounds; round++)
            for (var position = 1; position <= size >> round; position++)
            {
                var series = NewSeries(round == rounds ? stage.FinalBestOf : stage.BestOf, round, position, null, null, null);

                created[(round, position)] = series;
                stage.Series.Add(series);
            }

        ChampionshipSeries? third = null;

        if (stage.ThirdPlace && rounds >= 2)
        {
            third = NewSeries(stage.FinalBestOf, rounds, 2, null, null, null);
            third.ThirdPlace = true;
            stage.Series.Add(third);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var ((round, position), series) in created)
        {
            if (round < rounds)
            {
                series.NextSeriesId = created[(round + 1, (position + 1) / 2)].Id;
                series.NextSlotA = position % 2 == 1;
            }

            if (third != null && round == rounds - 1)
            {
                series.LoserNextSeriesId = third.Id;
                series.LoserNextSlotA = position % 2 == 1;
            }
        }

        foreach (var series in created.Where(pair => pair.Key.Round == 1).Select(pair => pair.Value))
            progress.Apply(championship, series, campaigns);
    }

    private void DrawRounds(ChampionshipStage stage, IReadOnlyList<ChampionshipCampaignOption> campaigns)
    {
        var eligible = CampaignRules.Eligible(stage, campaigns);

        if (eligible.Count == 0)
            eligible = campaigns.ToList();

        if (eligible.Count == 0)
            return;

        var order = ChampionshipShuffle.Shuffle(eligible, random.Next);
        var cursor = 0;

        foreach (var round in stage.Series.GroupBy(series => series.Round).OrderBy(group => group.Key))
        {
            var games = round.First().Games.Count;
            var picked = new List<int>();

            for (var index = 0; index < games; index++)
                picked.Add(order[cursor++ % order.Count].Id);

            foreach (var series in round)
                foreach (var game in series.Games.Where(game => game is { IsPlayed: false, IsInProgress: false }))
                {
                    game.CampaignId = picked[Math.Min(game.Number - 1, picked.Count - 1)];
                    game.CampaignSource = ChampionshipCampaignSource.Draw;
                    game.PickedByTeamId = null;
                }
        }
    }

    private static ChampionshipSeries NewSeries(int bestOf, int round, int position, int? groupId, int? teamA, int? teamB, bool games = true)
    {
        var series = new ChampionshipSeries
        {
            Round = round,
            Position = position,
            GroupId = groupId,
            TeamAId = teamA,
            TeamBId = teamB,
            BestOf = bestOf
        };

        if (games)
            for (var number = 1; number <= bestOf; number++)
                series.Games.Add(new ChampionshipGame { Number = number, Status = ChampionshipGameStatus.Pending });

        return series;
    }

    private async Task<(Championship Championship, ChampionshipStage Stage)> Load(int stageId, CancellationToken cancellationToken)
    {
        var championship = await loader.LoadByStageAsync(stageId, cancellationToken) ?? throw new ChampionshipException("The stage was not found.");

        return (championship, championship.Stages.First(stage => stage.Id == stageId));
    }
}
