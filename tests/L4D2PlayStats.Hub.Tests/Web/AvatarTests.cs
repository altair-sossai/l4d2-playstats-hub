using System.Net;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Players;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Web;
using L4D2PlayStats.Hub.Web.Infrastructure.Avatars;
using L4D2PlayStats.Hub.Web.Infrastructure.BackgroundServices;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class AvatarTests
{
    private const long Broken = 76561198000000301;
    private const long Working = 76561198000000302;

    [TestMethod]
    public async Task AnAvatarThatFailsToDownloadDoesNotStopTheOthers()
    {
        using var factory = new HubWebFactory();

        var handler = new RoutingHttpMessageHandler(uri =>
        {
            if (uri.Host == "steamcommunity.com")
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"<avatarFull><![CDATA[https://avatars.example/{uri.Segments[2].Trim('/')}.jpg]]></avatarFull>") };

            if (uri.AbsolutePath == $"/{Broken}.jpg")
                throw new HttpRequestException("connection reset");

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent([1, 2, 3]) };
        });

        var application = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHttpClient(AppConsts.SteamHttpClient).ConfigurePrimaryHttpMessageHandler(() => handler)));

        await AddProfilesAsync(application, Broken, Working);

        var service = ActivatorUtilities.CreateInstance<AvatarBackgroundService>(application.Services);

        await service.RefreshAsync(CancellationToken.None);

        var store = application.Services.GetRequiredService<AvatarStore>();
        var updated = await RunAsync(application, services => services.GetRequiredService<HubDbContext>().PlayerProfiles.AsNoTracking().Where(p => p.AvatarUpdatedAtUtc != null).Select(p => p.CommunityId).ToListAsync());

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(store.PathFor(Working)));
        Assert.IsFalse(File.Exists(store.PathFor(Broken)));
        CollectionAssert.AreEquivalent(new[] { Broken, Working }, updated);
        Assert.IsEmpty(Directory.GetFiles(Path.GetDirectoryName(store.PathFor(Working))!, "*" + AppConsts.AvatarTemporaryExtension));
    }

    [TestMethod]
    public void TheQueueTakesOnlySteamAccountsAndAsksForEachOneAgainOnlyAfterTheRetryTime()
    {
        var clock = new FakeClock();
        var queue = new AvatarQueue(clock);

        queue.Request(5);
        queue.Request(Working);
        queue.Request(Working);

        CollectionAssert.AreEqual(new[] { Working }, queue.Take(10).ToList());

        queue.Request(Working);
        Assert.IsEmpty(queue.Take(10));

        clock.Advance(TimeSpan.FromHours(AppConsts.AvatarRetryInHours + 1));
        queue.Request(Working);

        CollectionAssert.AreEqual(new[] { Working }, queue.Take(10).ToList());
    }

    private static Task AddProfilesAsync(WebApplicationFactory<Program> application, params long[] communityIds)
    {
        return RunAsync(application, async services =>
        {
            var dbContext = services.GetRequiredService<HubDbContext>();

            foreach (var communityId in communityIds)
                dbContext.PlayerProfiles.Add(new PlayerProfile { CommunityId = communityId, Name = communityId.ToString(), FirstSeenAtUtc = DateTime.UtcNow, LastSeenAtUtc = DateTime.UtcNow });

            return await dbContext.SaveChangesAsync();
        });
    }

    private static async Task<T> RunAsync<T>(WebApplicationFactory<Program> application, Func<IServiceProvider, Task<T>> action)
    {
        using var scope = application.Services.CreateScope();

        return await action(scope.ServiceProvider);
    }
}