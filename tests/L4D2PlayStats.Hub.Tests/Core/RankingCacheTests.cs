using L4D2PlayStats.Hub.Core.Infrastructure;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class RankingCacheTests
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    [TestMethod]
    public async Task ARankingIsCalculatedOnceAndLeavesTheMemoryWhenTheDataChanges()
    {
        using var hub = new TestHub();

        var server = (await hub.CreateServerAsync()).Server;

        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", Night, 1, 300, 200));
        await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", Night.AddMinutes(15), 2, 600, 500));

        var cache = (MemoryCache)hub.Scope().ServiceProvider.GetRequiredService<IMemoryCache>();
        var first = await hub.RunAsync(services => services.GetRequiredService<IRankingService>().AllTimeAsync());
        var cached = cache.Count;
        var second = await hub.RunAsync(services => services.GetRequiredService<IRankingService>().AllTimeAsync());

        Assert.AreSame(first, second);
        Assert.AreEqual(cached, cache.Count);

        hub.Scope().ServiceProvider.GetRequiredService<HubState>().DataChanged();
        cache.Compact(0);

        Assert.AreEqual(cached - 1, cache.Count);
        Assert.AreNotSame(first, await hub.RunAsync(services => services.GetRequiredService<IRankingService>().AllTimeAsync()));
    }
}
