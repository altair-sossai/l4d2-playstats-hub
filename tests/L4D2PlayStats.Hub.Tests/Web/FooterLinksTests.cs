using L4D2PlayStats.Hub.Web.Helpers;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class FooterLinksTests
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   \n  ")]
    public void WithoutLinksTheFooterPointsToTheProject(string? text)
    {
        var links = FooterLinks.Parse(text);

        Assert.HasCount(1, links);
        Assert.AreEqual(WebConsts.ProjectName, links[0].Text);
        Assert.AreEqual(WebConsts.ProjectUrl, links[0].Url);
    }

    [TestMethod]
    public void EachLineIsATextAndAnAddress()
    {
        var links = FooterLinks.Parse("Discord | https://discord.gg/example\r\n  Steam group|http://steamcommunity.com/groups/example  \n\n");

        Assert.HasCount(2, links);
        Assert.AreEqual("Discord", links[0].Text);
        Assert.AreEqual("https://discord.gg/example", links[0].Url);
        Assert.AreEqual("Steam group", links[1].Text);
        Assert.AreEqual("http://steamcommunity.com/groups/example", links[1].Url);
    }

    [TestMethod]
    public void OnlyTheFirstBarSeparatesTheTextFromTheAddress()
    {
        var links = FooterLinks.Parse("Search | https://example.com/?q=a|b");

        Assert.HasCount(1, links);
        Assert.AreEqual("Search", links[0].Text);
        Assert.AreEqual("https://example.com/?q=a|b", links[0].Url);
    }

    [TestMethod]
    [DataRow("Just a text")]
    [DataRow("| https://example.com")]
    [DataRow("Script | javascript:alert(1)")]
    [DataRow("File | ftp://example.com/file")]
    [DataRow("Relative | /players")]
    public void ALineThatIsNotALinkIsLeftOut(string line)
    {
        var links = FooterLinks.Parse($"{line}\nDiscord | https://discord.gg/example");

        Assert.HasCount(1, links);
        Assert.AreEqual("Discord", links[0].Text);
        Assert.IsTrue(FooterLinks.HasInvalidLines(line));
    }

    [TestMethod]
    public void WhenNoLineIsALinkTheFooterPointsToTheProject()
    {
        var links = FooterLinks.Parse("Just a text\nAnother text");

        Assert.HasCount(1, links);
        Assert.AreEqual(WebConsts.ProjectUrl, links[0].Url);
    }

    [TestMethod]
    public void NormalizingKeepsOnlyTheLinksOnePerLine()
    {
        var text = FooterLinks.Normalize("  Discord|https://discord.gg/example \r\n\r\nNot a link\nSteam | https://steamcommunity.com");

        Assert.AreEqual("Discord | https://discord.gg/example\nSteam | https://steamcommunity.com", text);
        Assert.IsNull(FooterLinks.Normalize("Not a link"));
        Assert.IsNull(FooterLinks.Normalize(null));
    }

    [TestMethod]
    public void TextWithOnlyLinksHasNoInvalidLines()
    {
        Assert.IsFalse(FooterLinks.HasInvalidLines("Discord | https://discord.gg/example\n\nSteam | https://steamcommunity.com"));
        Assert.IsFalse(FooterLinks.HasInvalidLines(null));
    }
}
