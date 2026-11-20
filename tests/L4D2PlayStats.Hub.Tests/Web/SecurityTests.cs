using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class SecurityTests
{
    private const string SteamKey = "0123456789ABCDEF0123456789ABCDEF";

    [TestMethod]
    public async Task EveryAnswerCarriesTheSecurityHeaders()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client();

        var response = await client.GetAsync("/");

        Assert.AreEqual("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.AreEqual("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.AreEqual("SAMEORIGIN", response.Headers.GetValues("X-Frame-Options").Single());
    }

    [TestMethod]
    public async Task TheSteamKeyIsNeverWrittenBackToThePage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new SiteSettings { Name = "Hub", SteamApiKey = SteamKey });

        using var administrator = factory.Client(HubWebFactory.Administrator);

        var page = await administrator.GetStringAsync("/admin/settings");

        Assert.DoesNotContain(SteamKey, page);
        Assert.Contains("id=\"Site_SteamApiKey\" name=\"Site.SteamApiKey\" type=\"password\" value=\"\"", page);
        Assert.Contains("name=\"RemoveSteamApiKey\"", page);
    }

    [TestMethod]
    public async Task AnEmptySteamKeyKeepsTheSavedOneAndRemoveClearsIt()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new SiteSettings { Name = "Hub", SteamApiKey = SteamKey });

        using var administrator = factory.Client(HubWebFactory.Administrator);

        await HubWebFactory.PostFormAsync(administrator, "/admin/settings", "/admin/settings", new Dictionary<string, string> { ["Site.Name"] = "Hub", ["Site.SteamApiKey"] = "" });

        var kept = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SiteSettings>());

        await HubWebFactory.PostFormAsync(administrator, "/admin/settings", "/admin/settings", new Dictionary<string, string> { ["Site.Name"] = "Hub", ["RemoveSteamApiKey"] = "true" });

        var removed = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<SiteSettings>());

        Assert.AreEqual(SteamKey, kept.SteamApiKey);
        Assert.IsNull(removed.SteamApiKey);
    }

    [TestMethod]
    public async Task GoogleAnalyticsIsNotLoadedInTheAdministration()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new SiteSettings { Name = "Hub", GoogleAnalyticsId = "G-TEST123" });

        using var administrator = factory.Client(HubWebFactory.Administrator);

        var home = await administrator.GetStringAsync("/");
        var admin = await administrator.GetStringAsync("/admin/settings");

        Assert.Contains("G-TEST123", home);
        Assert.DoesNotContain("googletagmanager", admin);
    }
}