using System.Net;
using System.Text.Json;
using L4D2PlayStats.Hub.Core.Modules.Patents.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Patents.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class PatentsAdminTests
{
    private const string Page = "/admin/patents";

    [TestMethod]
    public void TheDefaultScaleKeepsTheExperienceOfEveryLevel()
    {
        var scale = PatentCatalog.Default;

        Assert.HasCount(15, scale.All);
        Assert.AreEqual(0, scale.All[0].Experience);
        Assert.AreEqual(2150, scale.All[13].Experience);
        Assert.AreEqual(2500, scale.All[14].Experience);
        Assert.AreEqual("Global", scale.All[14].Name);
        Assert.AreEqual(15, scale.For(2500).Level);
        Assert.AreEqual(14, scale.For(2499).Level);
    }

    [TestMethod]
    public void AnotherHighestLevelMovesTheWholeCurve()
    {
        var scale = PatentCatalog.Scale(5000);

        Assert.AreEqual(5000, scale.All[14].Experience);
        Assert.AreEqual(4310, scale.All[13].Experience);
        Assert.AreEqual(14, scale.For(4999).Level);
        Assert.AreEqual(15, scale.For(5000).Level);
    }

    [TestMethod]
    public async Task AnAdministratorChangesTheExperienceOfTheHighestLevel()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var visitor = factory.Client();

        var before = await administrator.GetStringAsync(Page);
        var response = await HubWebFactory.PostFormAsync(administrator, Page, Page, new Dictionary<string, string> { ["MaxExperience"] = "5000" });
        var after = await administrator.GetStringAsync(Page);
        var patents = await visitor.GetStringAsync("/patents");
        var saved = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<PatentSettings>());

        Assert.Contains("value=\"2500\"", before);
        Assert.Contains("admin-form-actions", before);
        Assert.Contains("patent_icon_enabled", before);
        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(5000, saved.MaxExperience);
        Assert.Contains("value=\"5000\"", after);
        Assert.Contains("4,310", patents);
        Assert.Contains("5,000", patents);
    }

    [TestMethod]
    [DataRow("10", 100)]
    [DataRow("99999999", 1000000)]
    public async Task TheExperienceOfTheHighestLevelStaysWithinItsLimits(string value, int expected)
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var administrator = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, Page, Page, new Dictionary<string, string> { ["MaxExperience"] = value });

        var saved = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<PatentSettings>());

        Assert.AreEqual(expected, saved.MaxExperience);
    }

    [TestMethod]
    public async Task TheGameServerGetsTheLevelsOfTheNewScale()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true });

        var credentials = await factory.CreateServerAsync();

        using (var server = factory.ServerClient(credentials))
        {
            await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", CommunityTestHub.Night, 1, 300, 200));
            await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", CommunityTestHub.Night.AddMinutes(15), 2, 600, 500));
        }

        using var visitor = factory.Client();

        var low = await LevelsAsync(visitor);

        await factory.SaveAsync(new PatentSettings { MaxExperience = 100 });

        var high = await LevelsAsync(visitor);

        Assert.IsNotEmpty(low);
        Assert.IsTrue(high.Zip(low).All(pair => pair.First >= pair.Second));
        Assert.IsTrue(high.Zip(low).Any(pair => pair.First > pair.Second));
    }

    [TestMethod]
    public async Task OnlyAnAdministratorChangesThePatents()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var player = factory.Client(HubWebFactory.Player);

        await HubWebFactory.PostFormAsync(player, "/", Page, new Dictionary<string, string> { ["MaxExperience"] = "5000" });

        var saved = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<PatentSettings>());

        Assert.AreEqual(2500, saved.MaxExperience);
    }

    private static async Task<List<int>> LevelsAsync(HttpClient client)
    {
        var body = JsonDocument.Parse(await client.GetStringAsync("/api/players/patents")).RootElement;

        return [.. body.EnumerateArray().OrderBy(entry => entry.GetProperty("communityId").GetString()).Select(entry => entry.GetProperty("level").GetInt32())];
    }
}
