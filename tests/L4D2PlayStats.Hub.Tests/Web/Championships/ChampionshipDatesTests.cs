using L4D2PlayStats.Hub.Web.Helpers;

namespace L4D2PlayStats.Hub.Tests.Web.Championships;

[TestClass]
public class ChampionshipDatesTests
{
    private static readonly TimeZoneInfo ThreeHoursBehind = TimeZoneInfo.CreateCustomTimeZone("Test-03", TimeSpan.FromHours(-3), "Test-03", "Test-03");

    [TestMethod]
    public void AFormDateIsReadInTheTimeZoneOfTheSiteWithOrWithoutSeconds()
    {
        var expected = new DateTime(2026, 12, 4, 0, 30, 0, DateTimeKind.Utc);

        Assert.AreEqual(expected, ChampionshipDates.ToUtc("2026-12-03T21:30", ThreeHoursBehind));
        Assert.AreEqual(expected, ChampionshipDates.ToUtc("2026-12-03T21:30:00", ThreeHoursBehind));
        Assert.IsNull(ChampionshipDates.ToUtc("03/12/2026 21:30", ThreeHoursBehind));
        Assert.IsNull(ChampionshipDates.ToUtc(" ", ThreeHoursBehind));
    }

    [TestMethod]
    public void ADateGoesBackToTheFormInTheTimeZoneOfTheSite()
    {
        var utc = new DateTime(2026, 12, 4, 0, 30, 0, DateTimeKind.Utc);

        Assert.AreEqual("2026-12-03T21:30", ChampionshipDates.Input(utc, ThreeHoursBehind));
        Assert.AreEqual(string.Empty, ChampionshipDates.Input(null, ThreeHoursBehind));
        Assert.AreEqual("20261204T003000Z", ChampionshipDates.Calendar(utc));
    }

    [TestMethod]
    public void ATimeIsReadAndGoesBackToTheForm()
    {
        Assert.AreEqual(1230, ChampionshipDates.FromTimeInput("20:30"));
        Assert.IsNull(ChampionshipDates.FromTimeInput("25:00"));
        Assert.IsNull(ChampionshipDates.FromTimeInput(null));
        Assert.AreEqual("20:30", ChampionshipDates.TimeInput(1230));
        Assert.AreEqual(string.Empty, ChampionshipDates.TimeInput(null));
    }
}
