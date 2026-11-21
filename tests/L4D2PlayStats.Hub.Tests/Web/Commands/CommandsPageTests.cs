using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Tests.Support;

namespace L4D2PlayStats.Hub.Tests.Web.Commands;

[TestClass]
public class CommandsPageTests
{
    private const string PortugueseCookie = ".AspNetCore.Culture=c%3Dpt-BR%7Cuic%3Dpt-BR";
    private const string SpanishCookie = ".AspNetCore.Culture=c%3Des-ES%7Cuic%3Des-ES";

    [TestMethod]
    public async Task TheCommandsPageExplainsEverySection()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var response = await client.GetAsync("/commands");
        var body = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<title>Commands | ", body);
        Assert.Contains("id=\"cmd-search\"", body);
        Assert.Contains("href=\"#commands-typing\"", body);
        Assert.Contains("id=\"commands-typing\"", body);
        Assert.Contains("id=\"commands-rules\"", body);
        Assert.Contains("id=\"commands-admin\"", body);
        Assert.Contains("<code>!mix</code>", body);
        Assert.Contains("<code>!mix vorkyss Feeh</code>", body);
        Assert.Contains("<code>!kickspecs</code>", body);
        Assert.Contains("cmd-badge-when", body);
        Assert.Contains("cmd-badge-admin", body);
        Assert.Contains("class=\"panel-collapse collapse\"", body);
    }

    [TestMethod]
    public async Task TheMoreMenuLinksToTheCommandsPage()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        var home = await client.GetStringAsync("/");
        var menu = home[home.IndexOf("id=\"nav-more-menu\"", StringComparison.Ordinal)..];

        Assert.Contains("href=\"/commands\"", menu[..menu.IndexOf("</ul>", StringComparison.Ordinal)]);
        Assert.Contains("/imgs/menu/commands.svg", home);

        var page = (await client.GetStringAsync("/commands")).Replace("\r\n", "\n");

        Assert.Contains("<li class=\"active\">\n                            <a href=\"/commands\">", page);
    }

    [TestMethod]
    public async Task TheCommandsPageStaysWhenTheRankingIsOff()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = false });

        var home = await client.GetStringAsync("/");
        var response = await client.GetAsync("/commands");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Contains("href=\"/commands\"", home);
        Assert.DoesNotContain("href=\"/patents\"", home);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("<code>!ranking</code>", body);
        Assert.Contains("<code>!mix</code>", body);
    }

    [TestMethod]
    public async Task TheCommandsFollowTheModules()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, SkinShop = false, CommunityRanking = false, AntiCheat = false });

        var off = await client.GetStringAsync("/commands");

        Assert.Contains("<code>!ranking</code>", off);
        Assert.DoesNotContain("<code>!rmix</code>", off);
        Assert.DoesNotContain("<code>!skins</code>", off);
        Assert.DoesNotContain("id=\"commands-anticheat\"", off);
        Assert.DoesNotContain("<code>!aac vote</code>", off);

        await factory.SaveAsync(new ModuleSettings { Ranking = true, SkinShop = true, CommunityRanking = true, AntiCheat = true });

        var on = await client.GetStringAsync("/commands");

        Assert.Contains("<code>!rmix</code>", on);
        Assert.Contains("<code>!skins</code>", on);
        Assert.Contains("id=\"commands-anticheat\"", on);
        Assert.Contains("<code>!aac vote</code>", on);
        Assert.Contains("<code>!skin on</code>", on);
    }

    [TestMethod]
    [DataRow(PortugueseCookie, "Fila e vagas", "Como usar")]
    [DataRow(SpanishCookie, "Cola y plazas", "Buscar comandos")]
    public async Task TheCommandsAreInTheVisitorLanguage(string cookie, string section, string text)
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/commands");
        request.Headers.Add("Cookie", cookie);

        var body = await (await client.SendAsync(request)).Content.ReadAsStringAsync();

        Assert.Contains("<title>Comandos | ", body);
        Assert.Contains(section, body);
        Assert.Contains(text, body);
        Assert.DoesNotContain("How to type commands", body);
    }

    [TestMethod]
    public async Task TheCommandsPageIsReadableInTheLightTheme()
    {
        using var factory = new HubWebFactory();
        using var client = factory.Client();

        await factory.CompleteSetupAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/commands");
        request.Headers.Add("Cookie", "hub_theme=light");

        var body = await (await client.SendAsync(request)).Content.ReadAsStringAsync();

        Assert.Contains("/css/bootstrap.light.min.css", body);
        Assert.Contains("--cmd-name: #0b5f80", body);
    }
}
