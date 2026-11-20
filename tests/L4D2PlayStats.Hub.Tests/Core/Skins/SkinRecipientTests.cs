using L4D2PlayStats.Hub.Core.Modules.Skins.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Skins;

[TestClass]
public class SkinRecipientTests
{
    private const long Player = 76561198000000200;

    [TestMethod]
    [DataRow("76561198000000200")]
    [DataRow("  76561198000000200  ")]
    [DataRow("STEAM_0:0:19867236")]
    [DataRow("STEAM_1:0:19867236")]
    [DataRow("[U:1:39734472]")]
    [DataRow("U:1:39734472")]
    [DataRow("https://steamcommunity.com/profiles/76561198000000200")]
    [DataRow("http://steamcommunity.com/profiles/76561198000000200/")]
    public void ASteamIdIsReadInAnyFormat(string value)
    {
        var parsed = SkinRecipient.TryParse(value, out var communityId);

        Assert.IsTrue(parsed);
        Assert.AreEqual(Player, communityId);
    }

    [TestMethod]
    public void TheOddAccountOfASteamIdIsKept()
    {
        var parsed = SkinRecipient.TryParse("STEAM_0:1:19867236", out var communityId);

        Assert.IsTrue(parsed);
        Assert.AreEqual(Player + 1, communityId);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("123")]
    [DataRow("0000000000000000")]
    [DataRow("player")]
    [DataRow("STEAM_0:1")]
    [DataRow("[U:1:]")]
    [DataRow("https://steamcommunity.com/id/someone")]
    [DataRow("https://example.test/profiles/76561198000000200")]
    [DataRow("76561198000000200 or 1=1")]
    public void AnythingElseIsRefused(string? value)
    {
        var parsed = SkinRecipient.TryParse(value, out var communityId);

        Assert.IsFalse(parsed);
        Assert.AreEqual(0, communityId);
    }

    [TestMethod]
    public void TheShopSettingsStartWithTheRefundOnAndTheRequestsOfTheAssetsRepository()
    {
        var settings = new SkinShopSettings();

        Assert.IsTrue(settings.RefundAtSeasonEnd);
        Assert.IsNull(settings.AuthorContactUrl);
        Assert.AreEqual(CoreConsts.SkinAuthorRequestUrl, settings.AuthorRequestUrl);
    }
}