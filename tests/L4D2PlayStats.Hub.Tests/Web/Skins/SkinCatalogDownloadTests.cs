using System.Net;
using L4D2PlayStats.Hub.Core.Modules.Administrators;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using L4D2PlayStats.Hub.Core.Modules.Skins.Settings;
using L4D2PlayStats.Hub.Tests.Support;
using L4D2PlayStats.Hub.Tests.Support.Skins;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Tests.Web.Skins;

[TestClass]
public class SkinCatalogDownloadTests
{
    private const string CatalogUrl = "https://assets.example.test/skins/skins.json";

    [TestMethod]
    public async Task AnAdministratorImportsTheCatalogFromTheConfiguredAddress()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SkinSamples.Catalog());

        using var factory = new HubWebFactory();
        using var application = await StartAsync(factory, handler);
        using var client = application.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        client.DefaultRequestHeaders.Add(HubWebFactory.UserHeader, HubWebFactory.Administrator.ToString());

        var response = await HubWebFactory.PostFormAsync(client, "/admin/skins/catalog", "/admin/skins/catalog/fetch", []);
        var page = await client.GetStringAsync("/admin/skins/catalog");
        var skins = await RunAsync(application, services => services.GetRequiredService<ISkinService>().ListAsync());

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("alert-success", page);
        Assert.HasCount(SkinSamples.Count, skins);
        CollectionAssert.AreEqual(new[] { CatalogUrl }, handler.Requests);
    }

    [TestMethod]
    public async Task ACatalogThatCannotBeDownloadedChangesNothing()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.NotFound, "missing");

        using var factory = new HubWebFactory();
        using var application = await StartAsync(factory, handler);

        await RunAsync(application, services => services.GetRequiredService<ISkinCatalogService>().ImportAsync(SkinSamples.Catalog()));

        var result = await RunAsync(application, services => services.GetRequiredService<ISkinCatalogService>().ImportFromUrlAsync());
        var skins = await RunAsync(application, services => services.GetRequiredService<ISkinService>().ListAsync());

        Assert.AreEqual(SkinCatalogImportStatus.DownloadFailed, result.Status);
        Assert.HasCount(SkinSamples.Count, skins);
        Assert.IsTrue(skins.All(skin => skin.Available));
    }

    [TestMethod]
    public async Task ADownloadedFileThatIsNotACatalogIsRefused()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "<html>not a catalog</html>");

        using var factory = new HubWebFactory();
        using var application = await StartAsync(factory, handler);

        var result = await RunAsync(application, services => services.GetRequiredService<ISkinCatalogService>().ImportFromUrlAsync());
        var skins = await RunAsync(application, services => services.GetRequiredService<ISkinService>().ListAsync());

        Assert.AreEqual(SkinCatalogImportStatus.InvalidFile, result.Status);
        Assert.IsEmpty(skins);
    }

    private static async Task<WebApplicationFactory<Program>> StartAsync(HubWebFactory factory, StubHttpMessageHandler handler)
    {
        var application = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddHttpClient(CoreConsts.SkinCatalogHttpClient).ConfigurePrimaryHttpMessageHandler(() => handler)));

        await RunAsync(application, async services =>
        {
            await services.GetRequiredService<IAdministratorService>().AddAsync(HubWebFactory.Administrator, "Admin");
            await services.GetRequiredService<ISetupService>().CompleteAsync();
            await services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { SkinShop = true });
            await services.GetRequiredService<ISettingsService>().SaveAsync(new SkinShopSettings { CatalogUrl = CatalogUrl });

            return true;
        });

        return application;
    }

    private static async Task<T> RunAsync<T>(WebApplicationFactory<Program> application, Func<IServiceProvider, Task<T>> action)
    {
        using var scope = application.Services.CreateScope();

        return await action(scope.ServiceProvider);
    }
}
