using L4D2PlayStats.Hub.Web.Infrastructure.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class TelemetryTests
{
    [TestMethod]
    [DataRow("/")]
    [DataRow("/community")]
    [DataRow("/community/votes")]
    [DataRow("/player/76561198000000000")]
    [DataRow("/admin/backup")]
    [DataRow("/api/statistics")]
    [DataRow("/players")]
    [DataRow("/servers")]
    [DataRow("/servers/l4d2-competitive-1")]
    [DataRow("/api/servers")]
    public void ThePagesPeopleOpenAreTracked(string path)
    {
        Assert.IsTrue(HubTelemetry.IsTracked(new PathString(path)));
    }

    [TestMethod]
    [DataRow("/api/game-info/scoreboard")]
    [DataRow("/api/external-chat")]
    [DataRow("/api/servers/heartbeat")]
    [DataRow("/api/servers/l4d2-competitive-1/game-info/players")]
    [DataRow("/avatar/76561198000000000")]
    [DataRow("/css/bootstrap.min.css")]
    [DataRow("/js/command-builder.js")]
    [DataRow("/imgs/logo.png")]
    [DataRow("/api/server")]
    [DataRow("/server/feed")]
    [DataRow("/servers/l4d2-competitive-1/feed")]
    [DataRow("/servers/l4d2-competitive-1/players")]
    [DataRow("/servers/l4d2-competitive-1/header")]
    public void ThePollingOfTheGameServersAndTheFilesAreNotTracked(string path)
    {
        Assert.IsFalse(HubTelemetry.IsTracked(new PathString(path)));
    }

    [TestMethod]
    public void TheTelemetryIsOnOnlyWithAConnectionString()
    {
        var empty = new ConfigurationBuilder().Build();
        var configured = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { [WebConsts.ApplicationInsightsConnectionStringKey] = "InstrumentationKey=00000000-0000-0000-0000-000000000000" }).Build();

        Assert.IsFalse(HubTelemetry.IsConfigured(empty));
        Assert.IsTrue(HubTelemetry.IsConfigured(configured));
    }
}