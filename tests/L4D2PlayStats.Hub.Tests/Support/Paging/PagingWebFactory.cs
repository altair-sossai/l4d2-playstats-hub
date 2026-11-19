using L4D2PlayStats.Hub.Core.Modules.Community.Services;
using L4D2PlayStats.Hub.Core.Modules.Ranking.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace L4D2PlayStats.Hub.Tests.Support.Paging;

public class PagingWebFactory(int players) : HubWebFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRankingService>();
            services.AddSingleton<IRankingService>(new FakeRankingService(players));
            services.RemoveAll<ICommunityRankingService>();
            services.AddSingleton<ICommunityRankingService>(new FakeCommunityRankingService(players));
        });
    }
}
