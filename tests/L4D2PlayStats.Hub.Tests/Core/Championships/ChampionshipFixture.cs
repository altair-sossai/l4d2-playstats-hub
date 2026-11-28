using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

public static class ChampionshipFixture
{
    public const long FirstPlayer = 76561198100000000;
    public const long Admin = 76561198000000100;

    public static ChampionshipStageTemplate RoundRobin(int groups = 1, int qualifiers = 0, int bestOf = 1, ChampionshipCampaignRule rule = ChampionshipCampaignRule.RoundDraw)
    {
        return new ChampionshipStageTemplate("League", ChampionshipStageType.RoundRobin, groups, qualifiers, bestOf, rule, ChampionshipSeeding.Seeds);
    }

    public static ChampionshipStageTemplate Knockout(int bestOf = 3, ChampionshipSeeding seeding = ChampionshipSeeding.Seeds, ChampionshipCampaignRule rule = ChampionshipCampaignRule.Picks)
    {
        return new ChampionshipStageTemplate("Playoffs", ChampionshipStageType.Knockout, 1, 0, bestOf, rule, seeding);
    }

    public static Task<Championship> CreateAsync(TestHub hub, string name, params ChampionshipStageTemplate[] stages)
    {
        return hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().CreateAsync(name, stages, Admin));
    }

    public static async Task<List<int>> TeamsAsync(TestHub hub, int championshipId, int teams, int players = 4)
    {
        await hub.RunAsync(async services =>
        {
            var settings = services.GetRequiredService<IChampionshipService>();
            var championship = await Load(services, championshipId);

            await settings.UpdateAsync(championshipId, new ChampionshipSettingsForm { Name = championship.Name, PlayersPerGame = players, RosterSize = players, DraftMethod = championship.DraftMethod }, Admin);
        });

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().CreateTeamsAsync(championshipId, teams, "Team", Admin));

        var championship = await hub.RunAsync(services => Load(services, championshipId));
        var ordered = championship.Teams.OrderBy(team => team.Seed).ToList();

        for (var index = 0; index < ordered.Count; index++)
            for (var player = 0; player < players; player++)
            {
                var teamId = ordered[index].Id;
                var communityId = FirstPlayer + index * 100 + player;

                await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().AddMemberAsync(teamId, communityId, Admin));
            }

        foreach (var team in ordered)
        {
            var teamId = team.Id;
            var captain = FirstPlayer + ordered.IndexOf(team) * 100;

            await hub.RunAsync(services => services.GetRequiredService<IChampionshipTeamService>().SetLeadersAsync(teamId, captain, captain + 1, Admin));
        }

        return ordered.Select(team => team.Id).ToList();
    }

    public static Task<Championship> LoadAsync(TestHub hub, int championshipId)
    {
        return hub.RunAsync(services => Load(services, championshipId));
    }

    public static Task GenerateAsync(TestHub hub, int stageId)
    {
        return hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().GenerateAsync(stageId, Admin));
    }

    public static Task ResultAsync(TestHub hub, int gameId, int teamA, int teamB)
    {
        return hub.RunAsync(services => services.GetRequiredService<IChampionshipSeriesService>().SetManualResultAsync(gameId, teamA, teamB, null, Admin));
    }

    public static async Task PlaySeriesAsync(TestHub hub, int championshipId, int seriesId, bool teamAWins)
    {
        while (true)
        {
            var championship = await LoadAsync(hub, championshipId);
            var series = championship.Stages.SelectMany(stage => stage.Series).First(item => item.Id == seriesId);

            if (series.Finished)
                return;

            var game = series.Games.Where(item => item.Status == ChampionshipGameStatus.Pending).OrderBy(item => item.Number).First();

            await ResultAsync(hub, game.Id, teamAWins ? 600 : 300, teamAWins ? 300 : 600);
        }
    }

    public static ChampionshipSeries Series(Championship championship, int seriesId)
    {
        return championship.Stages.SelectMany(stage => stage.Series).First(series => series.Id == seriesId);
    }

    private static async Task<Championship> Load(IServiceProvider services, int championshipId)
    {
        return (await services.GetRequiredService<IChampionshipLoadService>().LoadAsync(championshipId, false))!;
    }
}
