using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class CampaignImageTests
{
    [TestMethod]
    [DataRow("Dead Center", "/imgs/campaigns/dead-center.jpg")]
    [DataRow("Hard Rain: Downpour", "/imgs/campaigns/hard-rain-downpour.jpg")]
    [DataRow("Dam It! The Director's Cut", "/imgs/campaigns/dam-it-the-directors-cut.jpg")]
    [DataRow("  Dark Carnival  -  Remix ", "/imgs/campaigns/dark-carnival-remix.jpg")]
    [DataRow("A campaign without a picture", "/imgs/campaigns/no-image.jpg")]
    [DataRow("", "/imgs/campaigns/no-image.jpg")]
    [DataRow(null, "/imgs/campaigns/no-image.jpg")]
    public void EveryPageFindsTheSamePictureForACampaign(string? campaign, string expected)
    {
        using var factory = new HubWebFactory();
        using var scope = factory.Services.CreateScope();

        Assert.AreEqual(expected, scope.ServiceProvider.GetRequiredService<ICampaignImageService>().Url(campaign));
    }
}
