using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Live.Services;
using L4D2PlayStats.Hub.Core.Modules.Live.Settings;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class LiveDelayPageTests
{
    [TestMethod]
    public async Task AnAdministratorSetsTheDelayOfTheLivePage()
    {
        using var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();

        using var client = factory.Client(HubWebFactory.Administrator);

        var response = await HubWebFactory.PostFormAsync(client, "/admin/servers", "/admin/servers/live", new Dictionary<string, string> { ["DelayInSeconds"] = "5" });
        var page = await client.GetStringAsync("/admin/servers");
        var settings = await factory.RunAsync(services => services.GetRequiredService<ISettingsService>().GetAsync<LiveServerSettings>());
        var delay = factory.Services.GetRequiredService<ILiveDelay>().Current;

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual(5, settings.DelayInSeconds);
        Assert.AreEqual(TimeSpan.FromSeconds(5), delay);
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
