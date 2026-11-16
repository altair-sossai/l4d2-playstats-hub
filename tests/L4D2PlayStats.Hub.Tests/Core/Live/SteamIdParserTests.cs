using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Live.Helpers;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class SteamIdParserTests
{
    private const long Alice = 76561198000000001;

    [TestMethod]
    [DataRow("76561198000000001")]
    [DataRow("  76561198000000001  ")]
    [DataRow("STEAM_1:1:19867136")]
    [DataRow("STEAM_0:1:19867136")]
    [DataRow("steam_1:1:19867136")]
    [DataRow("[U:1:39734273]")]
    [DataRow("U:1:39734273")]
    [DataRow("https://steamcommunity.com/profiles/76561198000000001")]
    [DataRow("http://steamcommunity.com/profiles/76561198000000001/")]
    public void EveryWayOfWritingASteamIdGivesTheSameAccount(string value)
    {
        Assert.AreEqual(Alice, SteamIdParser.Parse(value));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("alice")]
    [DataRow("12345")]
    [DataRow("1234567890123456")]
    [DataRow("https://steamcommunity.com/id/alice")]
    [DataRow("https://example.com/profiles/76561198000000001")]
    [DataRow("76561198000000001; drop")]
    public void WhatIsNotASteamIdIsRefused(string? value)
    {
        Assert.IsNull(SteamIdParser.Parse(value));
    }
}
