using L4D2PlayStats.Hub.Core.Modules.GameModes;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Helpers;
using L4D2PlayStats.Hub.Core.Modules.GameModes.Models;
using L4D2PlayStats.Hub.Core.Modules.StatisticsFiles.Helpers;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class GameModeTests
{
    [TestMethod]
    [DataRow("*", "ZoneMod v2.9.2b", true)]
    [DataRow("ZoneMod*", "ZoneMod v2.9.2b", true)]
    [DataRow("zonemod*", "ZoneMod v2.9.2b", true)]
    [DataRow("ZoneMod*", "3v3 Zonemod v2.9.2b", false)]
    [DataRow("*Zonemod*", "3v3 Zonemod v2.9.2b", true)]
    [DataRow("ZoneMod v?.*", "ZoneMod v2.9", true)]
    [DataRow("NeoMod*", "ZoneMod v2.9", false)]
    [DataRow("Pro Mod (Elite)*", "Pro Mod (Elite) 1.1", true)]
    [DataRow("", "ZoneMod v2.9", false)]
    public void PatternsIgnoreTheVersion(string pattern, string configuration, bool expected)
    {
        Assert.AreEqual(expected, WildcardPattern.IsMatch(pattern, configuration));
    }

    [TestMethod]
    public void TheFirstMatchingRuleDecides()
    {
        var rules = new GameModeRuleSet(
        [
            new GameModeRule { Id = 1, Name = "Retro does not count", ConfigurationPattern = "ZoneMod Retro*", CountsForRanking = false, Priority = 1 },
            new GameModeRule { Id = 2, Name = "ZoneMod 4v4", ConfigurationPattern = "ZoneMod*", TeamSize = 4, MinimumMaps = 2, Priority = 2 }
        ]);

        Assert.IsTrue(rules.CountsForRanking("ZoneMod v2.9.2b", 4, 2));
        Assert.IsFalse(rules.CountsForRanking("ZoneMod Retro v1", 4, 5));
        Assert.IsFalse(rules.CountsForRanking("ZoneMod v2.9.2b", 3, 5));
        Assert.IsFalse(rules.CountsForRanking("NeoMod Tourney 0.4a", 4, 5));
    }

    [TestMethod]
    [DataRow(1, false)]
    [DataRow(2, true)]
    [DataRow(5, true)]
    public void AMatchNeedsTheMinimumNumberOfMaps(int maps, bool expected)
    {
        var rules = new GameModeRuleSet([new GameModeRule { Name = "4v4", TeamSize = 4, MinimumMaps = 2 }]);

        Assert.AreEqual(expected, rules.CountsForRanking("ZoneMod v2.9.2b", 4, maps));
    }

    [TestMethod]
    public void WithoutRulesNothingCounts()
    {
        var rules = new GameModeRuleSet([]);

        Assert.IsFalse(rules.CountsForRanking("ZoneMod v2.9.2b", 4, 5));
    }

    [TestMethod]
    [DataRow("2026-11-15_21-30_0001_c2m1_highway.txt", 2026, 11, 15, 21, 30, 1)]
    [DataRow("2024-07-05_21-19_0999_c2m1_highway.txt", 2024, 7, 5, 21, 19, 999)]
    [DataRow("2024-07-05_21-19_1500_c2m1_highway.txt", 2024, 7, 5, 21, 19, 1500)]
    public void TheFileNameCarriesTheDate(string fileName, int year, int month, int day, int hour, int minute, int sequence)
    {
        var expected = new DateTime(year, month, day, hour, minute, 0).AddMilliseconds(sequence);

        Assert.AreEqual(expected, StatisticsFileName.ToDate(fileName));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("errors_20261115.log")]
    [DataRow("2026-13-15_21-30_0001_c2m1_highway.txt")]
    [DataRow("2026-11-15_25-30_0001_c2m1_highway.txt")]
    [DataRow("2026-11-15_21-30_0001_c2m1_highway.log")]
    public void OtherFileNamesHaveNoDate(string? fileName)
    {
        Assert.IsNull(StatisticsFileName.ToDate(fileName));
    }
}
