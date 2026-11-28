using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Enums;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Championships;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public class ChampionshipOptionsPagesTests
{
    private const long FirstPlayer = 76561198500000000;
    private const string Notice = "Everyone plays this championship with the Anti-Cheat.";

    [TestMethod]
    public async Task TheOptionsAreSavedInTheSettingsAndShownToThePlayersAndTheCaptains()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var admin = factory.Client(HubWebFactory.Administrator);

        var (championship, campaigns) = await SetupAsync(factory);
        var address = $"/admin/championships/{championship}";

        await HubWebFactory.PostFormAsync(admin, address, address, new Dictionary<string, string>
        {
            ["Name"] = "Cup",
            ["Slug"] = "cup",
            ["PlayersPerGame"] = "2",
            ["RosterSize"] = "2",
            ["DraftMethod"] = "Manual",
            ["status"] = "Running",
            ["SuggestedWeekday"] = "Sunday",
            ["suggestedTime"] = "20:00",
            ["AntiCheatRequired"] = "true",
            ["CampaignIds[0]"] = campaigns[0].Id.ToString(),
            ["CampaignIds[1]"] = campaigns[1].Id.ToString()
        });

        var saved = await ChampionshipWeb.LoadAsync(factory, championship);

        Assert.IsTrue(saved.AntiCheatRequired);
        Assert.AreEqual(DayOfWeek.Sunday, saved.SuggestedWeekday);
        Assert.AreEqual(20 * 60, saved.SuggestedMinute);
        CollectionAssert.AreEquivalent(new[] { campaigns[0].Id, campaigns[1].Id }, saved.Campaigns.Select(campaign => campaign.CampaignId).ToList());

        var settings = await admin.GetStringAsync(address);

        Assert.MatchesRegex($"name=\"CampaignIds\" value=\"{campaigns[0].Id}\"[^>]*checked=\"checked\"", settings);
        Assert.DoesNotMatchRegex($"name=\"CampaignIds\" value=\"{campaigns[2].Id}\"[^>]*checked=\"checked\"", settings);
        Assert.Contains("name=\"suggestedTime\" value=\"20:00\"", settings);

        using var visitor = factory.Client();

        var page = WebUtility.HtmlDecode(await visitor.GetStringAsync("/championships/cup"));

        Assert.Contains("Mandatory", page);
        Assert.Contains("Sunday at", page);
        Assert.Contains($"{campaigns[0].Name}, {campaigns[1].Name}", page);

        using var captain = factory.Client(FirstPlayer);

        var area = await captain.GetStringAsync("/championships/cup/captain?tab=series");
        var veto = Regex.Match(area, "id=\"captain-veto-\\d+\".*?</select>", RegexOptions.Singleline).Value;

        Assert.Contains(Notice, area);
        Assert.Contains("The suggestion of the championship is Sunday at", area);
        Assert.MatchesRegex("name=\"date\" required value=\"\\d{4}-\\d{2}-\\d{2}T20:00\"", area);
        Assert.Contains($"<option value=\"{campaigns[0].Id}\">", veto);
        Assert.DoesNotContain($"<option value=\"{campaigns[2].Id}\">", veto);
        Assert.Contains(Notice, await visitor.GetStringAsync($"/championships/cup/series/{saved.Stages.Single().Series.Single().Id}"));
    }

    [TestMethod]
    public async Task AChampionshipWithoutOptionsShowsNoneOfThem()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var visitor = factory.Client();

        await SetupAsync(factory);

        var page = await visitor.GetStringAsync("/championships/cup");

        Assert.DoesNotContain("Mandatory", page);
        Assert.DoesNotContain("Suggested time", page);
        Assert.DoesNotContain("championship-campaigns", page);
    }

    [TestMethod]
    public async Task ACampaignBelowTheMinimumOfMapsIsNotListedOnThePage()
    {
        using var factory = await ChampionshipWeb.FactoryAsync();
        using var visitor = factory.Client();

        var (championship, campaigns) = await SetupAsync(factory);
        var shortOne = campaigns.Single(campaign => campaign.Name == "The Last Stand");
        var longOne = campaigns.Single(campaign => campaign.Name == "Dark Carnival");

        await factory.RunAsync(async services =>
        {
            await services.GetRequiredService<IChampionshipService>().UpdateAsync(championship, new ChampionshipSettingsForm { Name = "Cup", PlayersPerGame = 2, RosterSize = 2, CampaignIds = [shortOne.Id, longOne.Id] }, null);

            var stage = (await ChampionshipWeb.LoadAsync(factory, championship)).Stages.Single();

            await services.GetRequiredService<IChampionshipStageService>().UpdateAsync(stage.Id, new ChampionshipStageForm { Name = stage.Name, MinimumMaps = 4 }, null);
        });

        var page = await visitor.GetStringAsync("/championships/cup");

        Assert.Contains(longOne.Name, page);
        Assert.DoesNotContain(shortOne.Name, page);
    }

    private static Task<(int Id, List<ChampionshipCampaignOption> Campaigns)> SetupAsync(HubWebFactory factory)
    {
        return factory.RunAsync(async services =>
        {
            var championships = services.GetRequiredService<IChampionshipService>();
            var teams = services.GetRequiredService<IChampionshipTeamService>();
            var stages = services.GetRequiredService<IChampionshipStageService>();
            var loader = services.GetRequiredService<IChampionshipLoadService>();
            var championship = await championships.CreateAsync("Cup", [new ChampionshipStageTemplate("Final", ChampionshipStageType.Knockout, 1, 0, 3, ChampionshipCampaignRule.Picks, ChampionshipSeeding.Seeds)], null);

            await championships.UpdateAsync(championship.Id, new ChampionshipSettingsForm { Name = "Cup", PlayersPerGame = 2, RosterSize = 2 }, null);

            for (var index = 0; index < 2; index++)
            {
                var team = await teams.CreateTeamAsync(championship.Id, $"Team {index}", null, null);

                await teams.SetLeadersAsync(team.Id, FirstPlayer + index * 10, FirstPlayer + index * 10 + 1, null);
            }

            var stage = (await loader.LoadAsync(championship.Id, false))!.Stages.Single();

            await stages.UpdateAsync(stage.Id, new ChampionshipStageForm { Name = stage.Name, Type = stage.Type, BestOf = 3, FinalBestOf = 3, CampaignRule = stage.CampaignRule, Seeding = stage.Seeding, Veto = true }, null);
            await stages.GenerateAsync(stage.Id, null);

            return (championship.Id, await loader.CampaignsAsync());
        });
    }
}
