using System.Net;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Administrators.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Campaigns.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public partial class AdminListsTests
{
    private const string AddButton = "<span class=\"glyphicon glyphicon-plus\"></span> Add";
    private const string SaveButton = "<span class=\"glyphicon glyphicon-floppy-disk\"></span> Save";
    private const string CancelButton = "<button type=\"button\" class=\"btn btn-default\" data-dismiss=\"modal\">Cancel</button>";
    private const string FormActions = "<div class=\"admin-form-actions\">";
    private const long FirstExtraAdministrator = 76561198100000000;

    [TestMethod]
    public async Task TheAdministratorsArePagedAndAddedFromAWindow()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.RunAsync(async services =>
        {
            var administrators = services.GetRequiredService<IAdministratorService>();

            for (var index = 0; index < 25; index++)
                await administrators.AddAsync(FirstExtraAdministrator + index, $"Extra {index}");
        });

        using var client = factory.Client(HubWebFactory.Administrator);

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/administrators"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/administrators?page=2"));
        var back = await HubWebFactory.PostFormAsync(client, "/admin/administrators", "/admin/administrators", new Dictionary<string, string> { ["communityId"] = (FirstExtraAdministrator + 30).ToString(), ["name"] = "Late", ["returnUrl"] = "/admin/administrators?page=2" });
        var outside = await HubWebFactory.PostFormAsync(client, "/admin/administrators", "/admin/administrators", new Dictionary<string, string> { ["communityId"] = (FirstExtraAdministrator + 31).ToString(), ["returnUrl"] = "https://example.com/" });

        Assert.AreEqual(20, AdministratorRow().Count(first));
        Assert.Contains("Showing 1–20 of 26", first);
        Assert.AreEqual(6, AdministratorRow().Count(second));
        Assert.Contains("Showing 21–26 of 26", second);
        Assert.Contains("name=\"returnUrl\" value=\"/admin/administrators?page=2\"", second);
        Assert.Contains("id=\"admin-edit-modal\"", first);
        Assert.Contains("data-admin-edit=\"\"", first);
        Assert.Contains(AddButton, first);
        Assert.Contains(CancelButton, first);
        Assert.Contains(SaveButton, first);
        Assert.Contains("data-player-picker=\"#communityId\" data-player-picker-name=\"#name\"", first);
        Assert.Contains("onsubmit=\"return confirm('Remove this administrator?')\"", first);
        Assert.DoesNotContain("panel-title\">Add an administrator", first);
        Assert.AreEqual("/admin/administrators?page=2", back.Headers.Location?.OriginalString);
        Assert.AreEqual("/admin/administrators", outside.Headers.Location?.OriginalString);
    }

    [TestMethod]
    public async Task ACampaignWithoutMapsOrARuleWithoutANameShowsTheError()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(client, "/admin/campaigns", "/admin/campaigns", new Dictionary<string, string> { ["Name"] = "No Maps", ["Maps"] = " , " });
        var campaigns = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/campaigns"));
        var saved = await factory.RunAsync(services => services.GetRequiredService<ICampaignService>().ListAsync());

        await HubWebFactory.PostFormAsync(client, "/admin/game-modes", "/admin/game-modes", new Dictionary<string, string> { ["Name"] = " ", ["ConfigurationPattern"] = "*" });
        var rules = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/game-modes"));

        Assert.Contains("Write the name of the campaign and at least one map.", campaigns);
        Assert.IsFalse(saved.Any(campaign => campaign.Name == "No Maps"));
        Assert.Contains("A name is required.", rules);
    }

    [TestMethod]
    public async Task TheCampaignsArePagedAndTheirMapsAreEditedAsAList()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var total = (await factory.RunAsync(services => services.GetRequiredService<ICampaignService>().PageAsync(1, 1))).Total;
        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/campaigns"));
        var last = WebUtility.HtmlDecode(await client.GetStringAsync($"/admin/campaigns?page={(total + 19) / 20}"));
        var saved = await HubWebFactory.PostFormAsync(client, "/admin/campaigns", "/admin/campaigns", new Dictionary<string, string> { ["Name"] = "Custom Run", ["Maps"] = "custom_m1\ncustom_m2\ncustom_m3", ["returnUrl"] = "/admin/campaigns?page=2" });
        var campaigns = await factory.RunAsync(services => services.GetRequiredService<ICampaignService>().ListAsync());
        var custom = campaigns.Single(campaign => campaign.Name == "Custom Run");
        var after = WebUtility.HtmlDecode(await client.GetStringAsync($"/admin/campaigns?page={(total + 20) / 20}"));

        Assert.IsGreaterThan(20, total);
        Assert.AreEqual(20, CampaignRow().Count(first));
        Assert.Contains($"Showing 1–20 of {total}", first);
        Assert.AreEqual(total - 20 * ((total - 1) / 20), CampaignRow().Count(last));
        Assert.DoesNotContain("<textarea", first);
        Assert.Contains("id=\"campaign-edit-modal\"", first);
        Assert.Contains("<input type=\"hidden\" id=\"campaign-maps\" name=\"Maps\" value=\"\">", first);
        Assert.Contains("class=\"cm-template\"", first);
        Assert.Contains("Add a map", first);
        Assert.Contains("c1m1_hotel → c1m4_atrium", first);
        Assert.Contains(AddButton, first);
        Assert.Contains(CancelButton, first);
        Assert.Contains(SaveButton, first);
        Assert.Contains("onsubmit=\"return confirm('Delete this campaign?')\"", first);
        Assert.AreEqual("/admin/campaigns?page=2", saved.Headers.Location?.OriginalString);
        CollectionAssert.AreEqual(new[] { "custom_m1", "custom_m2", "custom_m3" }, custom.Maps.OrderBy(map => map.Order).Select(map => map.MapName).ToList());
        Assert.Contains($"data-campaign-edit=\"{custom.Id}\" data-campaign-name=\"Custom Run\" data-campaign-maps=\"custom_m1\ncustom_m2\ncustom_m3\"", after);
    }

    [TestMethod]
    public async Task TheGameRulesAndTheSeenConfigurationsArePagedAndTheRulesAreEditedInAWindow()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.RunAsync(async services =>
        {
            var gameModes = services.GetRequiredService<IGameModeService>();

            for (var index = 0; index < 24; index++)
            {
                await gameModes.SaveRuleAsync(new GameModeRule { Name = $"Rule {index}", ConfigurationPattern = $"Config {index}*", Priority = index });
                await gameModes.RegisterSeenAsync($"Config {index}", 4);
            }

            await services.GetRequiredService<HubDbContext>().SaveChangesAsync();
        });

        using var client = factory.Client(HubWebFactory.Administrator);

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/game-modes"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/game-modes?tab=rules&page=2"));
        var seen = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/game-modes?tab=seen&seenPage=2"));
        var saved = await HubWebFactory.PostFormAsync(client, "/admin/game-modes", "/admin/game-modes", new Dictionary<string, string>
        {
            ["Id"] = "0",
            ["Name"] = "Practice",
            ["ConfigurationPattern"] = "*Practice*",
            ["TeamSize"] = "",
            ["MinimumMaps"] = "1",
            ["Priority"] = "-5",
            ["CountsForRanking"] = "false",
            ["returnUrl"] = "/admin/game-modes?tab=rules&page=2"
        });
        var practice = await factory.RunAsync(async services => (await services.GetRequiredService<IGameModeService>().ListRulesAsync()).Single(rule => rule.Name == "Practice"));

        Assert.AreEqual(20, RuleRow().Count(first));
        Assert.Contains("Showing 1–20 of 25", first);
        Assert.AreEqual(5, RuleRow().Count(second));
        Assert.Contains("Showing 21–25 of 25", second);
        Assert.Contains("Showing 21–24 of 24", seen);
        Assert.Contains("href=\"/admin/game-modes?tab=seen\"", seen);
        Assert.Contains("class=\"tab-pane active\" id=\"tab-seen\"", seen);
        Assert.DoesNotContain("form=\"rule-", first);
        Assert.Contains("id=\"rule-edit-modal\"", first);
        Assert.Contains("<label for=\"rule-pattern\">Configuration pattern <a href=\"#\" class=\"field-help field-help-more\"", first);
        Assert.Contains("<span class=\"label label-success\">Yes</span>", first);
        Assert.Contains(AddButton, first);
        Assert.Contains(CancelButton, first);
        Assert.Contains(SaveButton, first);
        Assert.Contains("onsubmit=\"return confirm('Delete this rule?')\"", first);
        Assert.AreEqual("/admin/game-modes?tab=rules&page=2", saved.Headers.Location?.OriginalString);
        Assert.IsNull(practice.TeamSize);
        Assert.IsFalse(practice.CountsForRanking);
        Assert.AreEqual(-5, practice.Priority);
    }

    [TestMethod]
    public async Task TheGameServersArePagedAndAddedFromAWindow()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        for (var index = 0; index < 21; index++)
            await factory.CreateServerAsync($"Server {index:00}");

        using var client = factory.Client(HubWebFactory.Administrator);

        var first = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/servers"));
        var second = WebUtility.HtmlDecode(await client.GetStringAsync("/admin/servers?tab=servers&page=2"));

        Assert.AreEqual(20, ServerRow().Count(first));
        Assert.Contains("Showing 1–20 of 21", first);
        Assert.AreEqual(1, ServerRow().Count(second));
        Assert.Contains("id=\"server-add-modal\"", first);
        Assert.Contains("data-target=\"#server-add-modal\"", first);
        Assert.DoesNotContain("panel-title\">Add a game server", first);
        Assert.DoesNotContain("> Create", first);
        Assert.Contains(AddButton, first);
        Assert.Contains(CancelButton, first);
        Assert.Contains(SaveButton, first);
        Assert.Contains("onsubmit=\"return confirm('Delete this server?')\"", first);
        Assert.Contains("name=\"returnUrl\" value=\"/admin/servers?tab=servers&page=2\"", second);
    }

    [TestMethod]
    public async Task EverySettingsFormEndsWithTheSameSaveButton()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, SiteChat = true, RemoteCommands = true, SkinShop = true, CommunityRanking = true, AntiCheat = true });

        var credentials = await factory.CreateServerAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        foreach (var page in new[] { "/admin/settings", "/admin/modules", "/admin/servers?tab=live", $"/admin/servers/{credentials.Server.Id}", "/admin/community", "/admin/backup?tab=storage", "/admin/chat-bans?tab=limits", "/admin/anti-cheat?tab=page", "/admin/skins/settings" })
        {
            var html = await client.GetStringAsync(page);

            Assert.Contains(FormActions, html, page);
            Assert.Contains($"<button class=\"btn btn-primary\" type=\"submit\">{SaveButton}</button>", html, page);
            Assert.DoesNotContain("panel-footer", html, page);
            Assert.DoesNotContain("glyphicon-ok\"></span> Save", html, page);
        }
    }

    [TestMethod]
    public async Task TheListsAddWithTheSameButtonAboveTheList()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, SiteChat = true, CommunityRanking = true, AntiCheat = true });

        using var client = factory.Client(HubWebFactory.Administrator);

        var pages = new Dictionary<string, string>
        {
            ["/admin/administrators"] = "admin-edit-modal",
            ["/admin/servers"] = "server-add-modal",
            ["/admin/campaigns"] = "campaign-edit-modal",
            ["/admin/game-modes"] = "rule-edit-modal",
            ["/admin/forced-names"] = "fn-edit-modal",
            ["/admin/chat-bans"] = "cb-add-modal",
            ["/admin/anti-cheat"] = "aca-add-modal",
            ["/admin/community?tab=blocked"] = "ca-block-add-modal"
        };

        foreach (var (page, modal) in pages)
        {
            var html = await client.GetStringAsync(page);

            Assert.Contains("<div class=\"admin-toolbar\">", html, page);
            Assert.Contains("<div class=\"admin-toolbar-actions\">", html, page);
            Assert.Contains(AddButton, html, page);
            Assert.Contains($"id=\"{modal}\"", html, page);
            Assert.Contains(CancelButton, html, page);
            Assert.Contains(SaveButton, html, page);
        }
    }

    [GeneratedRegex("data-admin-edit=\"\\d+\"")]
    private static partial Regex AdministratorRow();

    [GeneratedRegex("data-campaign-edit=\"\\d+\"")]
    private static partial Regex CampaignRow();

    [GeneratedRegex("data-rule-edit=\"\\d+\"")]
    private static partial Regex RuleRow();

    [GeneratedRegex("action=\"/admin/servers/\\d+/delete\"")]
    private static partial Regex ServerRow();
}
