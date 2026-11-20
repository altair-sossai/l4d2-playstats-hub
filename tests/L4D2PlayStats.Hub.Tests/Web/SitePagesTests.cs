using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Helpers;
using L4D2PlayStats.Hub.Web.Models.AntiCheat;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class SitePagesTests
{
    [TestMethod]
    public async Task TheHomePageUsesTheLookOfTheOldSite()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var body = await client.GetStringAsync("/");

        Assert.Contains("/css/bootstrap.min.css", body);
        Assert.Contains("navbar navbar-default", body);
        Assert.DoesNotContain("/lib/bootstrap/", body);
        Assert.DoesNotContain("data-bs-", body);
    }

    [TestMethod]
    public async Task TheThemeLinkSwitchesToTheLightTheme()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var response = await client.GetAsync("/theme/light?returnUrl=/patents");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/patents", response.Headers.Location?.ToString());
        Assert.IsTrue(response.Headers.GetValues("Set-Cookie").Any(cookie => cookie.StartsWith("hub_theme=light")));

        using var request = new HttpRequestMessage(HttpMethod.Get, "/patents");
        request.Headers.Add("Cookie", "hub_theme=light");

        var body = await (await client.SendAsync(request)).Content.ReadAsStringAsync();

        Assert.Contains("/css/bootstrap.light.min.css", body);
    }

    [TestMethod]
    public async Task TheThemeLinkNeverRedirectsToAnotherSite()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var response = await client.GetAsync("/theme/default?returnUrl=https://example.com/");

        Assert.AreEqual("/", response.Headers.Location?.ToString());
    }

    [TestMethod]
    public async Task ThePagesCarryTheirOwnMetadata()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var patents = await client.GetStringAsync("/patents");

        Assert.Contains("<title>Patents | ", patents);
        Assert.Contains("property=\"og:title\"", patents);
        Assert.Contains("/imgs/patents/", patents);
        Assert.Contains("rel=\"canonical\"", patents);
    }

    [TestMethod]
    public async Task TheHistoryListsAllTimeAndTheYears()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.Now.Date.AddHours(-3);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var history = await client.GetStringAsync("/history");
        var allTime = await client.GetStringAsync("/history/all-time");
        var unknown = await client.GetStringAsync("/history/1900");

        Assert.Contains("/history/all-time", history);
        Assert.Contains($"/history/{now.Year}", history);
        Assert.Contains("table-ranking", allTime);
        Assert.Contains("History not found", unknown);
    }

    [TestMethod]
    public async Task TheOldRankingAddressesLeadToTheHistory()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var allTime = await client.GetAsync("/ranking/all-time");
        var year = await client.GetAsync("/ranking/year/2026");

        Assert.AreEqual("/history/all-time", allTime.Headers.Location?.ToString());
        Assert.AreEqual("/history/2026", year.Headers.Location?.ToString());
    }

    [TestMethod]
    public async Task TheAvatarAlsoAnswersWithAnImageExtensionSoTheLightboxOpensItAsAnImage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var credentials = await factory.CreateServerAsync();
        using var server = factory.ServerClient(credentials);

        var now = DateTime.Now.Date.AddHours(-3);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", now, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", now.AddMinutes(15), 2, 600, 500));

        var response = await client.GetAsync($"/avatar/{StatisticsBuilder.PlayerA1}.jpg");
        var page = await client.GetStringAsync($"/player/{StatisticsBuilder.PlayerA1}");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/imgs/avatar-empty.png", response.Headers.Location?.ToString());
        Assert.Contains($"src=\"/avatar/{StatisticsBuilder.PlayerA1}.jpg\"", page);
    }

    [TestMethod]
    public async Task AnAvatarThatIsNotDownloadedYetFallsBackWithoutBeingCached()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var response = await client.GetAsync($"/avatar/{StatisticsBuilder.PlayerA1}");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/imgs/avatar-empty.png", response.Headers.Location?.ToString());
        Assert.IsTrue(response.Headers.CacheControl?.NoStore);
    }

    [TestMethod]
    [DataRow("L4D2 Competitive", "L4D2 Competitive")]
    [DataRow("Zone Server", "Zone Server competitive server")]
    public async Task TheAntiCheatKickerDoesNotRepeatTheWordCompetitive(string siteName, string expected)
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { AntiCheat = true });
        await factory.SaveAsync(new SiteSettings { Name = siteName });

        var body = await client.GetStringAsync("/anti-cheat");

        Assert.Contains($"<span class=\"aac-kicker\">{expected}</span>", body);
    }

    [TestMethod]
    public async Task TheAntiCheatPageOnlyExistsWhenItsModuleIsOn()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        Assert.AreEqual(HttpStatusCode.NotFound, (await client.GetAsync("/anti-cheat")).StatusCode);

        await factory.RunAsync(async services =>
        {
            var settings = services.GetRequiredService<ISettingsService>();
            var modules = await settings.GetAsync<ModuleSettings>();

            modules.AntiCheat = true;

            await settings.SaveAsync(modules);
            await settings.SaveAsync(new AntiCheatSettings { DownloadUrl = "https://example.com/anti-cheat.exe" });

            return true;
        });

        var body = await client.GetStringAsync("/anti-cheat");
        var home = await client.GetStringAsync("/");

        Assert.Contains("https://example.com/anti-cheat.exe", body);
        Assert.Contains("aac-hero", body);
        Assert.Contains("href=\"/anti-cheat\"", home);
    }

    [TestMethod]
    public void TheAntiCheatDownloadMustBeSecure()
    {
        var latest = new ClientVersionResponse { Version = "1.2.3", DownloadUrl = "http://example.com/a.exe", SizeBytes = 3145728 };
        var model = new AntiCheatModel(latest, "https://example.com/fallback.exe");

        Assert.AreEqual("https://example.com/fallback.exe", model.DownloadUrl);
        Assert.IsNotNull(model.SizeText);
        Assert.IsNull(new AntiCheatModel(null, "ftp://example.com/a.exe").DownloadUrl);
    }

    [TestMethod]
    public void FooterLinksAreReadOnePerLine()
    {
        var links = FooterLinks.Parse("Server repository | https://github.com/a/b\nbroken line\nSteam|https://steamcommunity.com/id/x\nBad | javascript:alert(1)");

        Assert.HasCount(2, links);
        Assert.AreEqual("Server repository", links[0].Text);
        Assert.AreEqual("https://steamcommunity.com/id/x", links[1].Url);
        Assert.HasCount(1, FooterLinks.Parse(null));
        Assert.HasCount(1, FooterLinks.Parse("nothing valid"));
    }
}
