using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public class ChampionshipMatchLinkTests
{
    [TestMethod]
    public async Task AMatchOfTheServerIsLinkedByItselfAndTheGameEndsAfterTheLastMap()
    {
        using var factory = await FactoryAsync();

        var night = DateTime.UtcNow.AddHours(-3);
        var first = StatisticsBuilder.Map("c1m1_hotel", night, 1, 300, 200);
        var (championshipId, gameId) = await SetupAsync(factory, first, "Dead Center", night.AddMinutes(40), 1);

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await HubWebFactory.UploadAsync(server, first);

        var game = await GameAsync(factory, gameId);

        Assert.AreEqual(ChampionshipGameStatus.InProgress, game.Status);
        Assert.IsTrue(game.AutoLinked);
        Assert.IsNotNull(game.MatchId);
        Assert.IsNotNull(game.LastStatisticsAtUtc);
        Assert.IsFalse((await SeriesAsync(factory, game.SeriesId)).Finished);

        using var visitor = factory.Client();

        var slug = (await LoadAsync(factory, championshipId)).Slug;
        var page = await visitor.GetStringAsync($"/championships/{slug}/series/{game.SeriesId}");

        Assert.Contains("In progress", page);
        Assert.Contains($"href=\"/matches/{game.MatchId}\"", page);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c1m2_streets", night.AddMinutes(20), 2, 600, 450));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c1m3_mall", night.AddMinutes(40), 3, 900, 700));

        game = await GameAsync(factory, gameId);

        Assert.AreEqual(ChampionshipGameStatus.InProgress, game.Status);
        Assert.IsGreaterThan(0, game.TeamAScore + game.TeamBScore);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c1m4_atrium", night.AddMinutes(60), 4, 1200, 1000));

        game = await GameAsync(factory, gameId);

        Assert.AreEqual(ChampionshipGameStatus.Played, game.Status);
        Assert.AreEqual(ChampionshipResultSource.Match, game.ResultSource);

        var series = await SeriesAsync(factory, game.SeriesId);

        Assert.IsTrue(series.Finished);
        Assert.IsNotNull(series.WinnerTeamId);

        var history = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipLogs.Where(log => log.ChampionshipId == championshipId).Select(log => log.Text).ToListAsync());

        Assert.Contains("Linked the match #{0} to game {1} of {2} by itself", history);
        Assert.Contains("Game {0} of {1} ended: every map was played", history);
    }

    [TestMethod]
    public async Task AGameThatEndsEarlyIsClosedAfterTheMinutesWithoutStatisticsOrByTheAdministrator()
    {
        using var factory = await FactoryAsync();

        var night = DateTime.UtcNow.AddHours(-2);
        var first = StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200);
        var (championshipId, gameId) = await SetupAsync(factory, first, "Dark Carnival", night, 3);
        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await HubWebFactory.UploadAsync(server, first);
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(20), 2, 500, 400));

        Assert.AreEqual(ChampionshipGameStatus.InProgress, (await GameAsync(factory, gameId)).Status);
        Assert.AreEqual(0, await factory.RunAsync(services => services.GetRequiredService<IChampionshipMatchService>().CloseIdleAsync()));

        await factory.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();
            var game = await dbContext.ChampionshipGames.SingleAsync(item => item.Id == gameId);

            game.LastStatisticsAtUtc = DateTime.UtcNow.AddMinutes(-61);
            await dbContext.SaveChangesAsync();
        });

        Assert.AreEqual(1, await factory.RunAsync(services => services.GetRequiredService<IChampionshipMatchService>().CloseIdleAsync()));

        var closed = await GameAsync(factory, gameId);

        Assert.AreEqual(ChampionshipGameStatus.Played, closed.Status);

        var series = await SeriesAsync(factory, closed.SeriesId);
        var second = series.Games.Single(item => item.Number == 2);

        Assert.IsFalse(series.Finished);

        await factory.RunAsync(async services =>
        {
            var campaigns = await services.GetRequiredService<IChampionshipLoadService>().CampaignsAsync();

            await services.GetRequiredService<IChampionshipSeriesService>().SetCampaignAsync(second.Id, campaigns.Single(campaign => campaign.Name == "Swamp Fever").Id, null);
        });

        var later = night.AddMinutes(90);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c3m1_plankcountry", later, 1, 200, 300));

        var live = await GameAsync(factory, second.Id);

        Assert.AreEqual(ChampionshipGameStatus.InProgress, live.Status);

        using var admin = factory.Client(HubWebFactory.Administrator);

        var url = $"/admin/championships/{championshipId}";
        var games = await admin.GetStringAsync($"{url}?tab=games");

        Assert.Contains($"data-game-end=\"{url}/series/{series.Id}/games/{second.Id}/end\"", games);
        Assert.Contains("btn-warning", games);

        var ended = await HubWebFactory.PostFormAsync(admin, $"{url}?tab=games", $"{url}/series/{series.Id}/games/{second.Id}/end", []);

        Assert.AreEqual(System.Net.HttpStatusCode.Redirect, ended.StatusCode);
        Assert.AreEqual(ChampionshipGameStatus.Played, (await GameAsync(factory, second.Id)).Status);

        await HubWebFactory.PostFormAsync(admin, $"{url}?tab=games", $"{url}/series/{series.Id}/games/{second.Id}", new Dictionary<string, string> { ["result"] = "none", ["campaignId"] = live.CampaignId!.Value.ToString(), ["originalCampaignId"] = live.CampaignId.Value.ToString() });

        var cleared = await GameAsync(factory, second.Id);

        Assert.AreEqual(ChampionshipGameStatus.Pending, cleared.Status);
        Assert.AreEqual(live.MatchId, cleared.UnlinkedMatchId);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c3m2_swamp", later.AddMinutes(20), 2, 400, 600));

        Assert.AreEqual(ChampionshipGameStatus.Pending, (await GameAsync(factory, second.Id)).Status);
    }

    [TestMethod]
    public async Task AMatchIsNotLinkedOutsideTheHoursOfToleranceNorWithAnotherCampaignNorWhenTheOptionIsOff()
    {
        using var factory = await FactoryAsync();

        var night = DateTime.UtcNow.AddHours(-30);
        var first = StatisticsBuilder.Map("c1m1_hotel", night, 1, 300, 200);
        var (championshipId, gameId) = await SetupAsync(factory, first, "Dead Center", night.AddHours(8), 1);
        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        await HubWebFactory.UploadAsync(server, first);

        Assert.AreEqual(ChampionshipGameStatus.Pending, (await GameAsync(factory, gameId)).Status);

        await factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var championship = (await services.GetRequiredService<IChampionshipLoadService>().LoadAsync(championshipId, false))!;

            await championships.UpdateAsync(championshipId, new ChampionshipSettingsForm { Name = championship.Name, PlayersPerGame = 4, RosterSize = 4, AutoLinkHours = 12, AutoLinkMatches = false }, null);
        });

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c1m2_streets", night.AddMinutes(20), 2, 600, 450));

        Assert.AreEqual(ChampionshipGameStatus.Pending, (await GameAsync(factory, gameId)).Status);

        await factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var championship = (await services.GetRequiredService<IChampionshipLoadService>().LoadAsync(championshipId, false))!;

            await championships.UpdateAsync(championshipId, new ChampionshipSettingsForm { Name = championship.Name, PlayersPerGame = 4, RosterSize = 4, AutoLinkHours = 12, AutoLinkMatches = true }, null);
        });

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", night.AddDays(-3), 1, 300, 200));

        Assert.AreEqual(ChampionshipGameStatus.Pending, (await GameAsync(factory, gameId)).Status);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c1m3_mall", night.AddMinutes(40), 3, 900, 700));

        Assert.AreEqual(ChampionshipGameStatus.InProgress, (await GameAsync(factory, gameId)).Status);
    }

    private static async Task<HubWebFactory> FactoryAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Championships = true });

        return factory;
    }

    private static Task<(int Championship, int Game)> SetupAsync(HubWebFactory factory, StatisticsBuilder sample, string campaign, DateTime scheduled, int bestOf)
    {
        return factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var teamService = services.GetRequiredService<IChampionshipTeamService>();
            var loader = services.GetRequiredService<IChampionshipLoadService>();
            var seriesService = services.GetRequiredService<IChampionshipSeriesService>();
            var championship = await championships.CreateAsync("Copa Automática", [new ChampionshipStageTemplate("Final", ChampionshipStageType.Knockout, 1, 0, bestOf, ChampionshipCampaignRule.Manual, ChampionshipSeeding.Seeds)], HubWebFactory.Administrator);

            await championships.UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Copa Automática", PlayersPerGame = 4, RosterSize = 4 }, null);

            var blue = await teamService.CreateTeamAsync(championship.Id, "Blue", null, null);
            var red = await teamService.CreateTeamAsync(championship.Id, "Red", null, null);

            foreach (var player in sample.Survivors('A'))
                await teamService.AddMemberAsync(blue.Id, player, null);

            foreach (var player in sample.Survivors('B'))
                await teamService.AddMemberAsync(red.Id, player, null);

            var stageId = (await loader.LoadAsync(championship.Id, false))!.Stages.Single().Id;

            await services.GetRequiredService<IChampionshipStageService>().GenerateAsync(stageId, null);

            var series = (await loader.LoadAsync(championship.Id, false))!.Stages.Single().Series.Single();
            var game = series.Games.Single(item => item.Number == 1);
            var campaigns = await loader.CampaignsAsync();

            await seriesService.SetScheduleAsync(series.Id, scheduled, null, null, null);
            await seriesService.SetCampaignAsync(game.Id, campaigns.Single(item => item.Name == campaign).Id, null);

            return (championship.Id, game.Id);
        });
    }

    private static Task<Championship> LoadAsync(HubWebFactory factory, int id)
    {
        return factory.RunAsync(async services => (await services.GetRequiredService<IChampionshipLoadService>().LoadAsync(id, false))!);
    }

    private static Task<ChampionshipGame> GameAsync(HubWebFactory factory, int gameId)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipGames.AsNoTracking().SingleAsync(game => game.Id == gameId));
    }

    private static Task<ChampionshipSeries> SeriesAsync(HubWebFactory factory, int seriesId)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().ChampionshipSeries.AsNoTracking().Include(series => series.Games).SingleAsync(series => series.Id == seriesId));
    }
}
