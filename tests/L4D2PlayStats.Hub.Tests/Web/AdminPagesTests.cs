using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Community.Enums;
using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Tests.Community.Support;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class AdminPagesTests
{
    private const string HtmxScript = "lib/htmx/htmx.min.js";

    private static readonly string[] Bootstrap5Marks = ["lib/bootstrap", "bootstrap.bundle", "data-bs-", "css/site.css", "form-check", "card-body", "text-bg-"];

    [TestMethod]
    public async Task TheAdministrationAndCommunityPagesUseTheSiteLayout()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1 });

        var credentials = await site.Factory.CreateServerAsync("Second Server");

        using var client = site.Factory.Client(HubWebFactory.Administrator);
        using var voter = site.Factory.Client(site.Voter);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        string[] pages =
        [
            "/admin", "/admin/servers", $"/admin/servers/{credentials.Server.Id}", "/admin/settings", "/admin/modules", "/admin/game-modes",
            "/admin/campaigns", "/admin/administrators", "/admin/community", "/community", CommunityWebSite.VotePage
        ];

        foreach (var page in pages)
        {
            var html = await (page.StartsWith("/admin") ? client : voter).GetStringAsync(page);

            Assert.Contains("css/bootstrap.min.css", html, page);
            Assert.Contains("navbar navbar-default", html, page);

            foreach (var mark in Bootstrap5Marks)
                Assert.DoesNotContain(mark, html, $"{page}: {mark}");
        }
    }

    [TestMethod]
    public async Task TheWizardUsesTheSiteLayoutInEveryStep()
    {
        using var factory = new HubWebFactory();
        using var anonymous = factory.Client();

        var first = await anonymous.GetStringAsync("/setup");

        await factory.RunAsync(services => services.GetRequiredService<IAdministratorService>().AddAsync(HubWebFactory.Administrator, "Admin"));

        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var signIn = await player.GetStringAsync("/setup");
        var steps = new List<string> { first, signIn };

        foreach (var step in new[] { "site", "modules", "server", "finish" })
            steps.Add(await administrator.GetStringAsync($"/setup?step={step}"));

        var created = await HubWebFactory.PostFormAsync(administrator, "/setup?step=server", "/setup/server", new Dictionary<string, string> { ["Name"] = "Wizard Server" });
        var connected = await administrator.GetStringAsync(created.Headers.Location!.OriginalString);

        steps.Add(connected);

        foreach (var html in steps)
        {
            Assert.Contains("css/bootstrap.min.css", html);
            Assert.Contains("wizard-steps", html);
            Assert.Contains("content=\"noindex, follow\"", html);

            foreach (var mark in Bootstrap5Marks)
                Assert.DoesNotContain(mark, html, mark);
        }

        Assert.Contains("action=\"/setup/start\"", first);
        Assert.Contains("href=\"/login?returnUrl=/setup\"", signIn);
        Assert.Contains("action=\"/setup/site\"", steps[2]);
        Assert.Contains("action=\"/setup/modules\"", steps[3]);
        Assert.Contains("action=\"/setup/server\"", steps[4]);
        Assert.Contains("action=\"/setup/finish\"", steps[5]);
        Assert.Contains("playstats_token \"wizard-server:", connected);
        Assert.Contains("hx-get=\"/admin/servers/", connected);
        Assert.Contains(HtmxScript, connected);
        Assert.DoesNotContain(HtmxScript, steps[4]);
    }

    [TestMethod]
    public async Task TheThemeCanBeChangedDuringTheSetup()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        var dark = await client.GetStringAsync("/setup");
        var change = await client.GetAsync("/theme/light?returnUrl=/setup");
        var light = await client.GetStringAsync("/setup");

        Assert.Contains("css/bootstrap.min.css", dark);
        Assert.AreEqual("/setup", change.Headers.Location!.OriginalString);
        Assert.Contains("css/bootstrap.light.min.css", light);
        Assert.DoesNotContain("css/bootstrap.min.css", light);
    }

    [TestMethod]
    public async Task TheMenuMarksTheAdministrationAndTheCommunityPages()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        using var client = site.Factory.Client(HubWebFactory.Administrator);

        var home = await client.GetStringAsync("/");

        Assert.IsFalse(AdminMenuActive().IsMatch(home));
        Assert.IsFalse(CommunityMenuActive().IsMatch(home));

        foreach (var page in new[] { "/admin", "/admin/settings", "/admin/campaigns", "/admin/community" })
        {
            var html = await client.GetStringAsync(page);

            Assert.IsTrue(AdminMenuActive().IsMatch(html), page);
            Assert.IsTrue(AdministrationItemActive().IsMatch(html), page);
            Assert.IsFalse(CommunityMenuActive().IsMatch(html), page);
        }

        foreach (var page in new[] { "/community", CommunityWebSite.VotePage })
        {
            var html = await client.GetStringAsync(page);

            Assert.IsTrue(CommunityMenuActive().IsMatch(html), page);
            Assert.IsFalse(AdminMenuActive().IsMatch(html), page);
        }
    }

    [TestMethod]
    public async Task OnlyThePagesThatRefreshThemselvesLoadHtmx()
    {
        using var site = new CommunityWebSite();

        await site.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1 });

        var credentials = await site.Factory.CreateServerAsync("Second Server");

        using var client = site.Factory.Client(HubWebFactory.Administrator);

        var server = await client.GetStringAsync($"/admin/servers/{credentials.Server.Id}");
        var vote = await client.GetStringAsync(CommunityWebSite.VotePage);

        Assert.Contains(HtmxScript, server);
        Assert.Contains($"hx-get=\"/admin/servers/{credentials.Server.Id}/status\"", server);
        Assert.Contains(HtmxScript, vote);

        foreach (var page in new[] { "/", "/admin", "/admin/servers", "/admin/settings", "/community" })
            Assert.DoesNotContain(HtmxScript, await client.GetStringAsync(page), page);
    }

    [TestMethod]
    public async Task TheAntiCheatModuleCanBeTurnedOnAndJoinsTheAdministrationMenu()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var modulesPage = await client.GetStringAsync("/admin/modules");
        var before = await client.GetStringAsync("/admin");

        await HubWebFactory.PostFormAsync(client, "/admin/modules", "/admin/modules", new Dictionary<string, string> { ["Ranking"] = "true", ["AntiCheat"] = "true" });

        var modules = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<ModuleSettings>());
        var after = await client.GetStringAsync("/admin/modules");
        var overview = await client.GetStringAsync("/admin");

        Assert.Contains("name=\"AntiCheat\" value=\"true\"", modulesPage);
        Assert.DoesNotContain("disabled", AntiCheatSwitch().Match(modulesPage).Value);
        Assert.DoesNotContain("Reserved. Not available yet.", modulesPage);
        Assert.DoesNotContain("href=\"/admin/anti-cheat\"", before);
        Assert.IsTrue(modules.AntiCheat);
        Assert.Contains("href=\"/admin/anti-cheat\"", overview);
        Assert.Contains("checked", AntiCheatSwitch().Match(after).Value);
    }

    [TestMethod]
    public async Task TheCommunityRankingShowsThePatentOnlyWithTheStatisticsModule()
    {
        using var with = new CommunityWebSite();
        using var without = new CommunityWebSite();

        await with.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1 });
        await without.StartAsync(new CommunityRankingSettings { MinimumMatchesToVote = 1, MinimumComparisonsToBeListed = 1 }, ranking: false);

        var withPatents = await RankingAfterOneVoteAsync(with);
        var withoutPatents = await RankingAfterOneVoteAsync(without);

        Assert.Contains("/imgs/patents/", withPatents);
        Assert.Contains("/imgs/first-place.png", withPatents);
        Assert.Contains("/imgs/second-place.png", withPatents);
        Assert.Contains("class=\"hover it-is-me\"", withPatents);
        Assert.DoesNotContain("/imgs/patents/", withoutPatents);
        Assert.Contains("/imgs/first-place.png", withoutPatents);
    }

    private static async Task<string> RankingAfterOneVoteAsync(CommunityWebSite site)
    {
        using var voter = site.Factory.Client(site.Voter);

        var pair = CommunityWebSite.Pair(await voter.GetStringAsync(CommunityWebSite.VotePage))!;

        await CommunityWebSite.PostVoteAsync(voter, pair, nameof(CommunityVoteOutcome.Left));

        using var winner = site.Factory.Client(pair.Left);

        return await winner.GetStringAsync("/community");
    }

    [GeneratedRegex("""<li class="dropdown active">\s*<a href="#"[^>]*>\s*<span class="glyphicon glyphicon-cog">""")]
    private static partial Regex AdminMenuActive();

    [GeneratedRegex("""<li class="active">\s*<a href="/admin">""")]
    private static partial Regex AdministrationItemActive();

    [GeneratedRegex("""<li class="active">\s*<a href="/community">""")]
    private static partial Regex CommunityMenuActive();

    [GeneratedRegex("""<input type="checkbox" id="module-AntiCheat"[^>]*>""")]
    private static partial Regex AntiCheatSwitch();
}
