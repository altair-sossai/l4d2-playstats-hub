using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Infrastructure.Extensions;
using L4D2PlayStats.Hub.Core.Infrastructure.Helpers;
using L4D2PlayStats.Hub.Core.Infrastructure.Models;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class PagingTests
{
    [TestMethod]
    [DataRow(0, 20, 1)]
    [DataRow(1, 20, 1)]
    [DataRow(20, 20, 1)]
    [DataRow(21, 20, 2)]
    [DataRow(40, 20, 2)]
    [DataRow(41, 20, 3)]
    public void TheNumberOfPagesNeverLeavesAnEmptyLastPage(int total, int size, int expected)
    {
        Assert.AreEqual(expected, Paging.TotalPages(total, size));
    }

    [TestMethod]
    public void APageKnowsItsRangeAndWhetherThereIsMore()
    {
        var middle = new Page<int>([21, 22], 2, 20, 45);
        var last = new Page<int>([41], 3, 20, 41);
        var empty = Page<int>.Empty(20);

        Assert.AreEqual(3, middle.TotalPages);
        Assert.IsTrue(middle.HasMore);
        Assert.AreEqual(21, middle.FirstItem);
        Assert.AreEqual(40, middle.LastItem);
        Assert.IsFalse(last.HasMore);
        Assert.AreEqual(41, last.LastItem);
        Assert.AreEqual(0, empty.FirstItem);
        Assert.AreEqual(0, empty.LastItem);
        Assert.AreEqual(1, empty.Number);
        Assert.IsFalse(empty.HasMore);
    }

    [TestMethod]
    public void MappingAPageKeepsItsNumbers()
    {
        var page = new Page<int>([1, 2], 2, 2, 5).Map(number => number * 10);

        CollectionAssert.AreEqual(new[] { 10, 20 }, page.Items.ToList());
        Assert.AreEqual(2, page.Number);
        Assert.AreEqual(5, page.Total);
    }

    [TestMethod]
    public async Task TheDatabaseReturnsOnlyTheRequestedPage()
    {
        using var hub = new TestHub();

        await SeedAsync(hub, 45);

        var second = await PageAsync(hub, 2);

        Assert.AreEqual(2, second.Number);
        Assert.AreEqual(45, second.Total);
        Assert.AreEqual(3, second.TotalPages);
        Assert.HasCount(20, second.Items);
        Assert.AreEqual("Player 20", second.Items[0].Name);
        Assert.IsTrue(second.HasMore);
    }

    [TestMethod]
    [DataRow(99)]
    [DataRow(3)]
    public async Task APageBeyondTheEndShowsTheLastPage(int requested)
    {
        using var hub = new TestHub();

        await SeedAsync(hub, 40);

        var page = await PageAsync(hub, requested);

        Assert.AreEqual(2, page.Number);
        Assert.HasCount(20, page.Items);
        Assert.AreEqual("Player 39", page.Items[^1].Name);
        Assert.IsFalse(page.HasMore);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-4)]
    public async Task APageBeforeTheStartShowsTheFirstPage(int requested)
    {
        using var hub = new TestHub();

        await SeedAsync(hub, 5);

        var page = await PageAsync(hub, requested);

        Assert.AreEqual(1, page.Number);
        Assert.HasCount(5, page.Items);
    }

    [TestMethod]
    public async Task AnEmptyTableGivesOneEmptyPage()
    {
        using var hub = new TestHub();

        var page = await PageAsync(hub, 4);

        Assert.AreEqual(1, page.Number);
        Assert.AreEqual(0, page.Total);
        Assert.IsEmpty(page.Items);
    }

    private static Task<Page<ForcedName>> PageAsync(TestHub hub, int page)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<ForcedName>().AsNoTracking().OrderBy(entry => entry.CommunityId).ToPageAsync(page, 20));
    }

    private static Task SeedAsync(TestHub hub, int count)
    {
        return hub.RunAsync(async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            for (var index = 0; index < count; index++)
                dbContext.Add(new ForcedName { CommunityId = CoreConsts.SteamCommunityIdBase + 1000 + index, Name = $"Player {index}", UpdatedAtUtc = hub.Clock.UtcNow });

            await dbContext.SaveChangesAsync();
        });
    }
}
