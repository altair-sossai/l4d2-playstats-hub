using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class AdminMenuTests
{
    private static readonly string[] EveryItem =
    [
        "/admin", "/admin/settings", "/admin/modules", "/admin/administrators", "/admin/backup",
        "/admin/servers", "/admin/remote-commands", "/admin/player-connections", "/admin/chat-bans",
        "/admin/game-modes", "/admin/campaigns", "/admin/community",
        "/admin/skins", "/admin/skins/prices", "/admin/skins/catalog", "/admin/skins/settings",
        "/admin/anti-cheat"
    ];

    private static readonly string[] OptionalItems =
    [
        "/admin/remote-commands", "/admin/player-connections", "/admin/chat-bans", "/admin/community",
        "/admin/skins", "/admin/skins/prices", "/admin/skins/catalog", "/admin/skins/settings", "/admin/anti-cheat"
    ];

    [TestMethod]
    public async Task EveryAdministrationPageShowsTheSideMenuWithItsOwnItemActive()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(AllModules());

        var credentials = await factory.CreateServerAsync("Menu Server");

        using var client = factory.Client(HubWebFactory.Administrator);

        var pages = new Dictionary<string, string>
        {
            ["/admin"] = "/admin",
            ["/admin/settings"] = "/admin/settings",
            ["/admin/modules"] = "/admin/modules",
            ["/admin/administrators"] = "/admin/administrators",
            ["/admin/backup"] = "/admin/backup",
            ["/admin/servers"] = "/admin/servers",
            [$"/admin/servers/{credentials.Server.Id}"] = "/admin/servers",
            ["/admin/remote-commands"] = "/admin/remote-commands",
            ["/admin/player-connections"] = "/admin/player-connections",
            ["/admin/chat-bans"] = "/admin/chat-bans",
            ["/admin/game-modes"] = "/admin/game-modes",
            ["/admin/campaigns"] = "/admin/campaigns",
            ["/admin/community"] = "/admin/community",
            ["/admin/skins"] = "/admin/skins",
            ["/admin/skins/prices"] = "/admin/skins/prices",
            ["/admin/skins/catalog"] = "/admin/skins/catalog",
            ["/admin/skins/settings"] = "/admin/skins/settings",
            ["/admin/anti-cheat"] = "/admin/anti-cheat"
        };

        foreach (var (page, expected) in pages)
        {
            var html = await client.GetStringAsync(page);
            var menu = Menu(html);
            var active = ActiveItem().Matches(menu).Select(match => match.Groups[1].Value).ToList();

            Assert.AreEqual(1, MenuStart().Matches(html).Count, page);
            Assert.Contains("class=\"admin-content\"", html, page);
            Assert.HasCount(1, active, page);
            Assert.AreEqual(expected, active[0], page);

            foreach (var item in EveryItem)
                Assert.Contains($"<a href=\"{item}\"", menu, $"{page}: {item}");
        }
    }

    [TestMethod]
    public async Task TheItemsOfTheModulesThatAreOffAreNotInTheMenu()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = false });

        using var client = factory.Client(HubWebFactory.Administrator);

        var menu = Menu(await client.GetStringAsync("/admin"));

        foreach (var item in EveryItem.Except(OptionalItems))
            Assert.Contains($"<a href=\"{item}\"", menu, item);

        foreach (var item in OptionalItems)
            Assert.DoesNotContain($"<a href=\"{item}\"", menu, item);

        Assert.DoesNotContain(">Skin shop<", menu);
        Assert.DoesNotContain(">Anti-Cheat<", menu);
    }

    [TestMethod]
    public async Task OnPhonesTheMenuOpensFromAControlThatNamesTheCurrentPage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(AllModules());

        using var client = factory.Client(HubWebFactory.Administrator);

        var prices = WebUtility.HtmlDecode(Menu(await client.GetStringAsync("/admin/skins/prices")));
        var servers = WebUtility.HtmlDecode(Menu(await client.GetStringAsync("/admin/servers")));

        Assert.Contains("data-toggle=\"collapse\" href=\"#admin-menu-items\"", prices);
        Assert.Contains("id=\"admin-menu-items\" class=\"collapse admin-menu-items\"", prices);
        Assert.Contains("<strong>Skin prices</strong>", prices);
        Assert.Contains("<strong>Game servers</strong>", servers);
    }

    [TestMethod]
    public async Task OnlyAdministratorsSeeTheMenuAndOnlyInTheAdministration()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(AllModules());

        using var anonymous = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        foreach (var page in new[] { "/admin", "/admin/servers", "/admin/chat-bans", "/admin/skins", "/admin/anti-cheat" })
        {
            var forbidden = await player.GetAsync(page);
            var unauthorized = await anonymous.GetAsync(page);

            Assert.AreEqual(HttpStatusCode.Forbidden, forbidden.StatusCode, page);
            Assert.AreEqual(HttpStatusCode.Unauthorized, unauthorized.StatusCode, page);
            Assert.DoesNotContain("admin-menu", await forbidden.Content.ReadAsStringAsync(), page);
            Assert.DoesNotContain("admin-menu", await unauthorized.Content.ReadAsStringAsync(), page);
        }

        foreach (var page in new[] { "/", "/servers", "/skins", "/community", "/anti-cheat" })
        {
            Assert.DoesNotContain("admin-menu", await player.GetStringAsync(page), page);
            Assert.DoesNotContain("admin-menu", await administrator.GetStringAsync(page), page);
        }
    }

    [TestMethod]
    public async Task TheTopMenuHasASingleAdminLink()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(AllModules());

        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        var home = await administrator.GetStringAsync("/");

        Assert.IsTrue(AdminLink("<li>").IsMatch(home));
        Assert.DoesNotContain("href=\"/admin/chat-bans\"", home);
        Assert.DoesNotContain("href=\"/admin\"", await player.GetStringAsync("/"));

        foreach (var page in new[] { "/admin", "/admin/chat-bans", "/admin/anti-cheat", "/admin/skins/prices" })
            Assert.IsTrue(AdminLink("<li class=\"active\">").IsMatch(await administrator.GetStringAsync(page)), page);
    }

    private static Regex AdminLink(string item)
    {
        return new Regex(Regex.Escape(item) + @"\s*<a href=""/admin"">\s*<span class=""glyphicon glyphicon-cog""></span> Admin\s*</a>\s*</li>");
    }

    [TestMethod]
    public async Task TheWizardHasNoAdministrationMenu()
    {
        using var factory = new HubWebFactory();
        using var anonymous = factory.Client();

        Assert.DoesNotContain("admin-menu", await anonymous.GetStringAsync("/setup"));
    }

    private static ModuleSettings AllModules()
    {
        return new ModuleSettings
        {
            Ranking = true,
            LiveServer = true,
            SkinShop = true,
            CommunityRanking = true,
            SiteChat = true,
            RemoteCommands = true,
            AntiCheat = true
        };
    }

    private static string Menu(string html)
    {
        var menu = MenuBlock().Match(html);

        Assert.IsTrue(menu.Success, "The page has no administration menu.");

        return menu.Value;
    }

    [GeneratedRegex("""<nav class="panel panel-default admin-menu"[\s\S]*?</nav>""")]
    private static partial Regex MenuBlock();

    [GeneratedRegex("<nav class=\"panel panel-default admin-menu\"")]
    private static partial Regex MenuStart();

    [GeneratedRegex("<li role=\"presentation\" class=\"active\">\\s*<a href=\"([^\"]+)\"")]
    private static partial Regex ActiveItem();
}
