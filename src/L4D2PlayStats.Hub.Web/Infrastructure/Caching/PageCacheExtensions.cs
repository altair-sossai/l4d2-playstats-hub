using L4D2PlayStats.Hub.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Caching;

public static class PageCacheExtensions
{
    public static IServiceCollection AddPageCache(this IServiceCollection services)
    {
        services.AddSingleton<CookieTempDataProvider>();
        services.Replace(ServiceDescriptor.Singleton<ITempDataProvider, QuietCookieTempDataProvider>());

        return services.AddOutputCache(options => options.AddPolicy(AppConsts.PageCachePolicy, new PageCachePolicy()));
    }

    public static WebApplication UsePageCache(this WebApplication app)
    {
        var hubState = app.Services.GetRequiredService<HubState>();
        var store = app.Services.GetRequiredService<IOutputCacheStore>();
        var data = ChangeToken.OnChange(() => hubState.DataChangeToken, () => Evict(store, AppConsts.PageCacheTag));
        var community = ChangeToken.OnChange(() => hubState.CommunityChangeToken, () => Evict(store, AppConsts.CommunityCacheTag));

        app.Lifetime.ApplicationStopping.Register(data.Dispose);
        app.Lifetime.ApplicationStopping.Register(community.Dispose);
        app.UseOutputCache();

        return app;
    }

    private static void Evict(IOutputCacheStore store, string tag)
    {
        store.EvictByTagAsync(tag, CancellationToken.None).AsTask().GetAwaiter().GetResult();
    }
}