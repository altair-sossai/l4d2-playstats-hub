using L4D2PlayStats.Hub.Web.Helpers;
using WebConsts = L4D2PlayStats.Hub.Web.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class QueryUrlTests
{
    [TestMethod]
    public void EmptyValuesAndTheFirstPageAreLeftOut()
    {
        Assert.AreEqual("/admin/forced-names", QueryUrl.Build("/admin/forced-names", ("q", null), ("page", 1)));
        Assert.AreEqual("/admin/forced-names", QueryUrl.Build("/admin/forced-names", ("q", "   "), ("page", 0)));
        Assert.AreEqual("/admin/forced-names?page=3", QueryUrl.Build("/admin/forced-names", ("q", ""), ("page", 3)));
    }

    [TestMethod]
    public void TextIsTrimmedAndEscaped()
    {
        Assert.AreEqual("/admin/chat-bans?q=Ellis%20%26%20Nick&page=2", QueryUrl.Build("/admin/chat-bans", ("q", "  Ellis & Nick "), ("page", 2)));
    }

    [TestMethod]
    public void NumbersKeepTheirOrderAndPagesCanHaveOtherNames()
    {
        Assert.AreEqual("/community/votes?voter=76561197960287930&player=5&page=4", QueryUrl.Build("/community/votes", ("voter", 76561197960287930L), ("player", 5L), ("page", 4)));
        Assert.AreEqual("/admin/game-modes?tab=seen&seenPage=2", QueryUrl.Build("/admin/game-modes", ("tab", "seen"), ("seenPage", 2)));
        Assert.AreEqual("/admin/game-modes?tab=seen", QueryUrl.Build("/admin/game-modes", ("tab", "seen"), ("seenPage", 1)));
    }

    [TestMethod]
    public void APathWithAQueryGetsTheNextParameters()
    {
        Assert.AreEqual("/a?b=1&c=2", QueryUrl.Build("/a?b=1", ("c", 2L)));
    }

    [TestMethod]
    public void TheOldUrlBuildersKeepTheirAddresses()
    {
        Assert.AreEqual("/admin/community?tab=voters", CommunityAdminUrls.Voters(1));
        Assert.AreEqual("/admin/community?tab=voters&page=2", CommunityAdminUrls.Voters(2));
        Assert.AreEqual("/admin/community?tab=blocked&blockedPage=3", CommunityAdminUrls.Blocked(3));
        Assert.AreEqual("/admin/community?tab=settings", CommunityAdminUrls.Of(WebConsts.CommunitySettingsTab));
        Assert.AreEqual("/admin/forced-names?q=Ellis&page=2", ForcedNameUrls.Page(" Ellis ", 2));
        Assert.AreEqual("/admin/forced-names", ForcedNameUrls.Page(null, 1));
        Assert.AreEqual("/admin/campaigns?page=2", AdminListUrls.Page(WebConsts.AdminCampaignsUrl, 2));
        Assert.AreEqual("/admin/campaigns", AdminListUrls.Page(WebConsts.AdminCampaignsUrl, 1));
        Assert.AreEqual("/admin/servers?tab=servers&page=2", AdminListUrls.TabPage(WebConsts.AdminServersUrl, WebConsts.ServersListTab, WebConsts.AdminPageQuery, 2));
        Assert.AreEqual("/admin/servers?tab=live", AdminTabs.Url(WebConsts.AdminServersUrl, WebConsts.ServersLiveTab));
        Assert.AreEqual("/community/votes", CommunityVoteUrls.Votes(null, null));
        Assert.AreEqual("/community/votes?player=76561197960287930&page=2", CommunityVoteUrls.Votes(null, 76561197960287930, 2));
    }
}
