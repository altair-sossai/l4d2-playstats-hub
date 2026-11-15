using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Ranking;
using L4D2PlayStats.Hub.Core.Modules.Servers;
using L4D2PlayStats.Hub.Core.Modules.Settings;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Support.Skins;

public static class SkinWebFactoryExtensions
{
    public static async Task StartSkinShopAsync(this HubWebFactory factory, bool importCatalog = true)
    {
        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { SkinShop = true });

        if (importCatalog)
            await factory.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportAsync(SkinSamples.Catalog()));
    }

    public static async Task<long> PlayerWithStarsAsync(this HubWebFactory factory, GameServerCredentials credentials)
    {
        using var server = factory.ServerClient(credentials);

        var now = DateTime.UtcNow;
        var night = new DateTime(now.Year, now.Month, Math.Min(now.Day, 20), 12, 0, 0);

        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", night, 1, 300, 200));
        await HubWebFactory.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", night.AddMinutes(15), 2, 600, 500));

        var stars = await factory.RunAsync(services => services.GetRequiredService<IRankingService>().StarsAsync());

        return stars.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;
    }

    public static async Task<long> PlayerWithStarsAsync(this HubWebFactory factory)
    {
        return await factory.PlayerWithStarsAsync(await factory.CreateServerAsync());
    }

    public static Task<T> ShopAsync<T>(this HubWebFactory factory, Func<ISkinShopService, Task<T>> action)
    {
        return factory.RunAsync(services => action(services.GetRequiredService<ISkinShopService>()));
    }

    public static Task<List<Skin>> SkinsAsync(this HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<ISkinService>().ListAsync());
    }

    public static Task<List<SkinPurchase>> PurchasesAsync(this HubWebFactory factory)
    {
        return factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<SkinPurchase>().AsNoTracking().ToListAsync());
    }

    public static async Task<HttpResponseMessage> PostFileAsync(HttpClient client, string formPage, string action, string fileName, string content)
    {
        var html = await client.GetStringAsync(formPage);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"");
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(content));

        file.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var form = new MultipartFormDataContent
        {
            { new StringContent(match.Groups[1].Value), "__RequestVerificationToken" },
            { file, "file", fileName }
        };

        return await client.PostAsync(action, form);
    }
}
