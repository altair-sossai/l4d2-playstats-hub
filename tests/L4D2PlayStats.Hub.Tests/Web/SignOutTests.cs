using System.Net;
using L4D2PlayStats.Hub.Tests.Support;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class SignOutTests
{
    [TestMethod]
    public async Task TheMenuSignsOutWithAHiddenFormThatCarriesTheAntiforgeryToken()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Player);

        var html = await client.GetStringAsync("/matches?page=1");

        Assert.Contains("<form id=\"logout-form\" method=\"post\" action=\"/logout?returnUrl=%2Fmatches%3Fpage%3D1\" style=\"display: none\">", html);
        Assert.Contains("onclick=\"document.getElementById('logout-form').submit(); return false;\"", html);
        Assert.Contains("/imgs/logout.png", html);
        Assert.DoesNotContain("href=\"/logout", html);
    }

    [TestMethod]
    public async Task AVisitorDoesNotGetTheSignOutForm()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client();

        var html = await client.GetStringAsync("/matches");

        Assert.DoesNotContain("logout-form", html);
        Assert.Contains("/login?returnUrl=", html);
    }

    [TestMethod]
    public async Task SigningOutWithTheTokenClearsTheCookieAndGoesBackToTheLocalPage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Player);

        var response = await HubWebFactory.PostFormAsync(client, "/matches", "/logout?returnUrl=%2Fmatches%3Fpage%3D2", []);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/matches?page=2", response.Headers.Location!.OriginalString);
        Assert.IsTrue(response.Headers.GetValues("Set-Cookie").Any(cookie => cookie.StartsWith(".AspNetCore.Cookies=;", StringComparison.Ordinal)));
    }

    [TestMethod]
    public async Task SigningOutNeverRedirectsToAnotherSite()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Player);

        var external = await HubWebFactory.PostFormAsync(client, "/matches", "/logout?returnUrl=https%3A%2F%2Fevil.example%2F", []);
        var protocolRelative = await HubWebFactory.PostFormAsync(client, "/matches", "/logout?returnUrl=%2F%2Fevil.example", []);

        Assert.AreEqual("/", external.Headers.Location!.OriginalString);
        Assert.AreEqual("/", protocolRelative.Headers.Location!.OriginalString);
    }

    [TestMethod]
    public async Task AnotherSiteCannotSignTheVisitorOut()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Player);

        var get = await client.GetAsync("/logout?returnUrl=/");
        var withoutToken = await client.PostAsync("/logout?returnUrl=/", new FormUrlEncodedContent([]));

        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, get.StatusCode);
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutToken.StatusCode);
        Assert.IsFalse(get.Headers.TryGetValues("Set-Cookie", out var getCookies) && getCookies.Any(cookie => cookie.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal)));
        Assert.IsFalse(withoutToken.Headers.TryGetValues("Set-Cookie", out var postCookies) && postCookies.Any(cookie => cookie.StartsWith(".AspNetCore.Cookies=", StringComparison.Ordinal)));
    }
}