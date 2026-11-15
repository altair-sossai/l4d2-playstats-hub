using System.Net;
using System.Net.Http.Json;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinModuleTests
{
    private static readonly string[] AdminPages = ["/admin/skins", "/admin/skins/prices", "/admin/skins/catalog", "/admin/skins/settings"];

    [TestMethod]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(false, false)]
    public async Task WithTheShopOrTheRankingOffTheShopStopsAnswering(bool skinShop, bool ranking)
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        var credentials = await factory.CreateServerAsync();

        await factory.SaveAsync(new ModuleSettings { SkinShop = skinShop, Ranking = ranking });

        using var visitor = factory.Client();
        using var administrator = factory.Client(HubWebFactory.Administrator);
        using var server = factory.ServerClient(credentials);

        foreach (var page in new[] { "/skins", "/api/skins", "/api/players/skins", $"/api/players/{HubWebFactory.Player}/wallet" })
            Assert.AreEqual(HttpStatusCode.NotFound, (await visitor.GetAsync(page)).StatusCode, page);

        foreach (var page in AdminPages)
            Assert.AreEqual(HttpStatusCode.NotFound, (await administrator.GetAsync(page)).StatusCode, page);

        var equip = await server.PostAsJsonAsync($"/api/players/{HubWebFactory.Player}/skins/equip", new { weapon = SkinSamples.Smg, skin = SkinSamples.DesertSmg });
        var home = await visitor.GetStringAsync("/");
        var admin = await administrator.GetStringAsync("/admin");

        Assert.AreEqual(HttpStatusCode.NotFound, equip.StatusCode);
        Assert.DoesNotContain("href=\"/skins\"", home);
        Assert.DoesNotContain("href=\"/admin/skins\"", admin);
    }

    [TestMethod]
    public async Task WithTheShopOnItsRoutesAnswerAndTheMenusShowIt()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var visitor = factory.Client();
        using var administrator = factory.Client(HubWebFactory.Administrator);

        foreach (var page in new[] { "/skins", "/api/skins", "/api/players/skins", $"/api/players/{HubWebFactory.Player}/wallet" })
            Assert.AreEqual(HttpStatusCode.OK, (await visitor.GetAsync(page)).StatusCode, page);

        var home = await visitor.GetStringAsync("/");
        var admin = await administrator.GetStringAsync("/admin");

        Assert.Contains("href=\"/skins\"", home);
        Assert.Contains("href=\"/admin/skins\"", admin);
    }

    [TestMethod]
    public async Task TheSkinPagesOfTheAdministrationAreOnlyForAdministrators()
    {
        using var factory = new HubWebFactory();

        await factory.StartSkinShopAsync();

        using var anonymous = factory.Client();
        using var player = factory.Client(HubWebFactory.Player);
        using var administrator = factory.Client(HubWebFactory.Administrator);

        foreach (var page in AdminPages)
        {
            Assert.AreEqual(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(page)).StatusCode, page);
            Assert.AreEqual(HttpStatusCode.Forbidden, (await player.GetAsync(page)).StatusCode, page);
            Assert.AreEqual(HttpStatusCode.OK, (await administrator.GetAsync(page)).StatusCode, page);
        }
    }
}
