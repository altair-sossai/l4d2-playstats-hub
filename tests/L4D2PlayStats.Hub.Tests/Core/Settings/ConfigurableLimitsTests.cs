using L4D2PlayStats.Hub.Core.Modules.Community.Settings;
using L4D2PlayStats.Hub.Core.Modules.Matches.Helpers;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core.Settings;

[TestClass]
public class ConfigurableLimitsTests
{
    [TestMethod]
    public void TheRankingSettingsStartWithTheOldValuesAndStayInRange()
    {
        var defaults = new RankingSettings();
        var settings = new RankingSettings { MatchGapInHours = 0, DefaultPunishment = -5, HomeListedPlayers = 9999, HomeVisiblePlayers = 9999, MinimumMatches = -1, MatchesPerPage = 1, PlayerRecentMatches = 5000 };

        settings.Normalize();

        Assert.AreEqual(6, defaults.MatchGapInHours);
        Assert.AreEqual(150, defaults.DefaultPunishment);
        Assert.AreEqual(50, defaults.HomeVisiblePlayers);
        Assert.AreEqual(100, defaults.HomeListedPlayers);
        Assert.AreEqual(CoreConsts.MatchMinGapInHours, settings.MatchGapInHours);
        Assert.AreEqual(1, settings.DefaultPunishment);
        Assert.AreEqual(CoreConsts.RankingMaxListedPlayers, settings.HomeListedPlayers);
        Assert.AreEqual(settings.HomeListedPlayers, settings.HomeVisiblePlayers);
        Assert.AreEqual(0, settings.MinimumMatches);
        Assert.AreEqual(50, defaults.MatchesPerPage);
        Assert.AreEqual(50, defaults.PlayerRecentMatches);
        Assert.AreEqual(CoreConsts.MatchListMinSize, settings.MatchesPerPage);
        Assert.AreEqual(CoreConsts.MatchListMaxSize, settings.PlayerRecentMatches);
    }

    [TestMethod]
    public void TheGapDecidesWhereOneMatchEndsAndTheNextStarts()
    {
        var start = new DateTime(2026, 11, 14, 20, 0, 0);
        DateTime[] dates = [start, start.AddHours(1), start.AddHours(4), start.AddHours(5)];

        var wide = FileClusters.Around(dates, start, TimeSpan.FromHours(6));
        var narrow = FileClusters.Around(dates, start, TimeSpan.FromHours(2));

        Assert.AreEqual(start.AddHours(5), wide!.Value.End);
        Assert.AreEqual(start.AddHours(1), narrow!.Value.End);
    }

    [TestMethod]
    public void TheOtherNamesOfTheVotingStayInRange()
    {
        var settings = new CommunityRankingSettings { OtherNamesShown = 99 };

        settings.Normalize();

        Assert.AreEqual(4, new CommunityRankingSettings().OtherNamesShown);
        Assert.AreEqual(CoreConsts.CommunityMaxBallotNames, settings.OtherNamesShown);
    }

    [TestMethod]
    public void ThePrivacySettingsKeepThreeMonthsOfConnectionsByDefault()
    {
        Assert.AreEqual(3, new PrivacySettings().ConnectionRetentionInMonths);
    }
}