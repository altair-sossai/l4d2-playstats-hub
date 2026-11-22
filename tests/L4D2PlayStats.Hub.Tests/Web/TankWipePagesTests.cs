using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class TankWipePagesTests
{
    private const string TankIcon = "/imgs/infected-type/tank.jpg";

    [TestMethod]
    public async Task EveryPageWithMvpsAlsoShowsTheTankWipes()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (wiper, wipes) = await UploadMatchAsync(factory);
        var match = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Matches.AsNoTracking().SingleAsync());
        var file = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().StatisticsFiles.AsNoTracking().OrderBy(f => f.FileDate).FirstAsync());

        var home = await client.GetStringAsync("/");
        var players = await client.GetStringAsync("/players");
        var byTankWipes = WebUtility.HtmlDecode(await client.GetStringAsync("/players?orderBy=TankWipes&asc=False"));
        var player = await client.GetStringAsync($"/player/{wiper}");
        var list = await client.GetStringAsync("/matches");
        var details = await client.GetStringAsync($"/matches/{match.Id}");
        var map = await client.GetStringAsync($"/matches/{match.Id}/statistics/{file.Id}");
        var inGame = await client.GetStringAsync($"/ingame?steamid={wiper}");
        var inGamePlayer = await client.GetStringAsync($"/ingame/player?steamid={wiper}");

        Assert.Contains("Tank wipes", home);
        Assert.Contains("Tank wipes", players);
        Assert.Contains("/players?orderBy=TankWipes&amp;asc=False", players);
        Assert.Contains("Tank wipes", byTankWipes);
        Assert.Contains("Tank wipes", player);
        Assert.Contains("Round halves in which the player, as Tank, wiped the survivors and was still alive when the round ended.", player);
        Assert.Contains(TankIcon, list);
        Assert.Contains("Tank wipes", details);
        Assert.Contains(TankIcon, map);
        Assert.Contains("Tank wipes", inGame);
        Assert.Contains("Tank wipes", inGamePlayer);
        Assert.Contains($"<div class=\"big\">{wipes}</div>", inGamePlayer);
    }

    [TestMethod]
    public async Task TheMatchCardShowsOneTankPerTankWipe()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var (_, _) = await UploadMatchAsync(factory);
        var total = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().MatchPlayers.SumAsync(p => p.TankWipes));
        var list = await client.GetStringAsync("/matches");

        Assert.IsGreaterThan(0, total);
        Assert.AreEqual(total, Regex.Count(list, Regex.Escape(TankIcon)));
    }

    [TestMethod]
    public async Task TheAdministratorSetsTheExperienceOfATankWipeAndThePatentsPageShowsIt()
    {
        using var factory = new HubWebFactory();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();

        var form = await administrator.GetStringAsync("/admin/settings");

        Assert.Contains("name=\"Ranking.TankWipe\" type=\"number\" value=\"50\"", form);
        Assert.Contains("Tank wipe: +50 XP points", WebUtility.HtmlDecode(await visitor.GetStringAsync("/patents")));

        await HubWebFactory.PostFormAsync(administrator, "/admin/settings", "/admin/settings", new Dictionary<string, string> { ["Site.Name"] = "Hub", ["Ranking.TankWipe"] = "4" });

        Assert.AreEqual(4, (await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<RankingSettings>())).TankWipe);
        Assert.Contains("Tank wipe: +4 XP points", WebUtility.HtmlDecode(await visitor.GetStringAsync("/patents")));

        await factory.SaveAsync(new RankingSettings { TankWipe = 0 });

        Assert.DoesNotContain("Tank wipe:", WebUtility.HtmlDecode(await visitor.GetStringAsync("/patents")));
    }

    private static async Task<(long Wiper, int Wipes)> UploadMatchAsync(HubWebFactory factory)
    {
        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.UtcNow;
        var night = new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);
        var first = StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200);
        var last = StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 600, 500);

        await HubWebFactory.UploadAsync(server, first);
        await HubWebFactory.UploadAsync(server, last);

        var top = TankWipes.Count([first.Statistics, last.Statistics]).MaxBy(pair => pair.Value);

        return (top.Key, top.Value);
    }
}
