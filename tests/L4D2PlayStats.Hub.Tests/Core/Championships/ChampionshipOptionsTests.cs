using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Exceptions;
using L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Championships.Models;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;
using static L4D2PlayStats.Hub.Tests.Core.Championships.ChampionshipFixture;

namespace L4D2PlayStats.Hub.Tests.Core.Championships;

[TestClass]
public class ChampionshipOptionsTests
{
    [TestMethod]
    public async Task OnlyTheCampaignsOfTheChampionshipCanBeChosenVetoedOrDrawn()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());

        await TeamsAsync(hub, championship.Id, 2);

        var campaigns = await CampaignsAsync(hub);
        var allowed = campaigns.Take(5).Select(campaign => campaign.Id).ToList();

        await SaveAsync(hub, championship.Id, new ChampionshipSettingsForm { CampaignIds = [.. allowed, int.MaxValue] });

        var stage = (await LoadAsync(hub, championship.Id)).Stages.Single();

        await hub.RunAsync(services => services.GetRequiredService<IChampionshipStageService>().UpdateAsync(stage.Id, new ChampionshipStageForm { Name = stage.Name, Type = stage.Type, BestOf = 3, FinalBestOf = 3, CampaignRule = stage.CampaignRule, Seeding = stage.Seeding, Veto = true, OncePerTeam = true }, Admin));
        await GenerateAsync(hub, stage.Id);

        var series = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.Single();
        var games = series.Games.OrderBy(game => game.Number).ToList();
        var captainA = FirstPlayer;
        var captainB = FirstPlayer + 100;
        var refused = campaigns[6].Id;

        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => RunAsync(hub, service => service.VetoAsync(series.Id, refused, captainA)));
        await RunAsync(hub, service => service.VetoAsync(series.Id, allowed[0], captainA));
        await RunAsync(hub, service => service.VetoAsync(series.Id, allowed[1], captainB));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => RunAsync(hub, service => service.PickAsync(games[0].Id, refused, captainA)));
        await Assert.ThrowsExactlyAsync<ChampionshipException>(() => RunAsync(hub, service => service.SetCampaignAsync(games[0].Id, refused, Admin)));
        await RunAsync(hub, service => service.PickAsync(games[0].Id, allowed[2], captainA));
        await RunAsync(hub, service => service.PickAsync(games[1].Id, allowed[3], captainB));
        await ResultAsync(hub, games[0].Id, 600, 400);
        await ResultAsync(hub, games[1].Id, 300, 400);

        var decider = Series(await LoadAsync(hub, championship.Id), series.Id).Games.Single(game => game.Number == 3);

        Assert.AreEqual(allowed[4], decider.CampaignId);
    }

    [TestMethod]
    public async Task ADrawOfTheRoundsUsesOnlyTheCampaignsOfTheChampionship()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "League", RoundRobin());

        await TeamsAsync(hub, championship.Id, 4);

        var allowed = (await CampaignsAsync(hub)).Where(campaign => campaign.Maps >= 4).Skip(3).Take(2).Select(campaign => campaign.Id).ToList();

        await SaveAsync(hub, championship.Id, new ChampionshipSettingsForm { CampaignIds = allowed });
        await GenerateAsync(hub, (await LoadAsync(hub, championship.Id)).Stages.Single().Id);

        var drawn = (await LoadAsync(hub, championship.Id)).Stages.Single().Series.SelectMany(series => series.Games).Select(game => game.CampaignId!.Value).Distinct().ToList();

        CollectionAssert.AreEquivalent(allowed, drawn);
    }

    [TestMethod]
    public async Task ADeletedCampaignLeavesTheChampionshipAndNoneSelectedAllowsAll()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());
        var campaigns = await CampaignsAsync(hub);

        await SaveAsync(hub, championship.Id, new ChampionshipSettingsForm { CampaignIds = [campaigns[0].Id, campaigns[1].Id] });
        await hub.RunAsync(services => services.GetRequiredService<ICampaignService>().DeleteAsync(campaigns[0].Id));

        CollectionAssert.AreEqual(new[] { campaigns[1].Id }, (await LoadAsync(hub, championship.Id)).Campaigns.Select(campaign => campaign.CampaignId).ToList());

        await SaveAsync(hub, championship.Id, new ChampionshipSettingsForm());

        var loaded = await LoadAsync(hub, championship.Id);

        Assert.IsEmpty(loaded.Campaigns);
        Assert.HasCount(campaigns.Count - 1, CampaignRules.Allowed(loaded, await CampaignsAsync(hub)));
    }

    [TestMethod]
    public async Task TheOriginalCampaignsOfTheGameAreRecognized()
    {
        using var hub = new TestHub();

        var original = (await CampaignsAsync(hub)).Where(campaign => campaign.Original).Select(campaign => campaign.Name).ToList();

        CollectionAssert.AreEquivalent(new[] { "Dead Center", "Dark Carnival", "Swamp Fever", "Hard Rain", "The Parish", "The Passing", "The Sacrifice", "No Mercy", "Crash Course", "Death Toll", "Dead Air", "Blood Harvest", "Cold Stream", "The Last Stand" }, original);
    }

    [TestMethod]
    public async Task TheAntiCheatAndTheSuggestedTimeAreSavedAndAWeekdayNeedsATime()
    {
        using var hub = new TestHub();

        var championship = await CreateAsync(hub, "Cup", Knockout());

        await SaveAsync(hub, championship.Id, new ChampionshipSettingsForm { AntiCheatRequired = true, SuggestedWeekday = DayOfWeek.Sunday, SuggestedMinute = 20 * 60 });

        var loaded = await LoadAsync(hub, championship.Id);

        Assert.IsTrue(loaded.AntiCheatRequired);
        Assert.AreEqual(DayOfWeek.Sunday, loaded.SuggestedWeekday);
        Assert.AreEqual(20 * 60, loaded.SuggestedMinute);

        await SaveAsync(hub, championship.Id, new ChampionshipSettingsForm { SuggestedWeekday = DayOfWeek.Sunday, SuggestedMinute = 24 * 60 });

        loaded = await LoadAsync(hub, championship.Id);

        Assert.IsFalse(loaded.AntiCheatRequired);
        Assert.IsNull(loaded.SuggestedWeekday);
        Assert.IsNull(loaded.SuggestedMinute);
    }

    [TestMethod]
    public void TheSuggestedDateIsTheNextOneInsideTheChampionship()
    {
        var championship = new Championship { SuggestedWeekday = DayOfWeek.Sunday, SuggestedMinute = 20 * 60, StartsOn = new DateTime(2026, 12, 1), EndsOn = new DateTime(2026, 12, 31) };

        Assert.AreEqual(new DateTime(2026, 12, 6, 20, 0, 0), SuggestedDates.Next(championship, new DateTime(2026, 11, 28, 16, 0, 0)));
        Assert.AreEqual(new DateTime(2026, 12, 13, 20, 0, 0), SuggestedDates.Next(championship, new DateTime(2026, 12, 6, 20, 0, 0)));
        Assert.IsNull(SuggestedDates.Next(championship, new DateTime(2026, 12, 28, 10, 0, 0)));

        championship.SuggestedWeekday = null;

        Assert.AreEqual(new DateTime(2026, 12, 28, 20, 0, 0), SuggestedDates.Next(championship, new DateTime(2026, 12, 28, 10, 0, 0)));

        championship.SuggestedMinute = null;

        Assert.IsNull(SuggestedDates.Next(championship, new DateTime(2026, 12, 2)));
    }

    private static Task<List<ChampionshipCampaignOption>> CampaignsAsync(TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<IChampionshipLoadService>().CampaignsAsync());
    }

    private static Task RunAsync(TestHub hub, Func<IChampionshipSeriesService, Task> action)
    {
        return hub.RunAsync(services => action(services.GetRequiredService<IChampionshipSeriesService>()));
    }

    private static Task SaveAsync(TestHub hub, int id, ChampionshipSettingsForm form)
    {
        form.Name = "Cup";

        return hub.RunAsync(services => services.GetRequiredService<IChampionshipService>().UpdateAsync(id, form, Admin));
    }
}
