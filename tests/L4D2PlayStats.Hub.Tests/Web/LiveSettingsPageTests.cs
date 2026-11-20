using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Live.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class LiveSettingsPageTests
{
    [TestMethod]
    public async Task AnAdministratorSetsTheDelayOfTheLivePage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(client, "/admin/servers", "/admin/servers/live", new Dictionary<string, string> { ["DelayInSeconds"] = "5", ["RefreshInMilliseconds"] = "4000", ["OnlineWindowInSeconds"] = "120", ["RemoteCommandExpirationInMinutes"] = "10" });
        var page = await client.GetStringAsync("/admin/servers");
        var settings = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<LiveServerSettings>());
        var live = factory.Services.GetRequiredService<ILiveSettings>().Current;

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(5, settings.DelayInSeconds);
        Assert.AreEqual(5, live.DelayInSeconds);
        Assert.AreEqual(4000, settings.RefreshInMilliseconds);
        Assert.AreEqual(120, live.OnlineWindowInSeconds);
        Assert.AreEqual(10, live.RemoteCommandExpirationInMinutes);
        Assert.Contains("name=\"DelayInSeconds\" type=\"number\" min=\"0\" max=\"600\" value=\"5\"", page);
    }

    [TestMethod]
    public async Task OnlyAnAdministratorSetsTheDelay()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var player = factory.Client(HubWebFactory.Player);

        await player.PostAsync("/admin/servers/live", new FormUrlEncodedContent(new Dictionary<string, string> { ["DelayInSeconds"] = "0" }));

        var settings = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<LiveServerSettings>());

        Assert.AreEqual(15, settings.DelayInSeconds);
    }
}