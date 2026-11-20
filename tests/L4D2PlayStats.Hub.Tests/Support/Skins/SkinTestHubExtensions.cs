using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using L4D2PlayStats.Hub.Core.Modules.Skins;
using L4D2PlayStats.Hub.Core.Modules.Skins.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Skins.Services.Contracts;

namespace L4D2PlayStats.Hub.Tests.Support.Skins;

public static class SkinTestHubExtensions
{
    private static readonly DateTime Night = new(2026, 11, 14, 21, 0, 0);

    public static Task<SkinCatalogImportResult> ImportSkinsAsync(this TestHub hub, string? json = null)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISkinCatalogService>().ImportAsync(json ?? SkinSamples.Catalog()));
    }

    public static Task EnableSkinShopAsync(this TestHub hub, bool ranking = true)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISettingsService>().SaveAsync(new ModuleSettings { Ranking = ranking, SkinShop = true }));
    }

    public static async Task<long> PlayerWithStarsAsync(this TestHub hub, int matches = 1)
    {
        var server = (await hub.CreateServerAsync()).Server;

        for (var match = 0; match < matches; match++)
        {
            var start = Night.AddDays(-match);

            await hub.UploadAsync(server, StatisticsBuilder.Map("c2m1_highway", start, 1, 300, 200));
            await hub.UploadAsync(server, StatisticsBuilder.Map("c2m2_fairgrounds", start.AddMinutes(15), 2, 600, 500));
        }

        var stars = await hub.StarsAsync();

        return stars.OrderByDescending(pair => pair.Value).ThenBy(pair => pair.Key).First().Key;
    }

    public static Task<Dictionary<long, int>> StarsAsync(this TestHub hub)
    {
        return hub.RunAsync(StarsOracle.CountAsync);
    }

    public static Task<T> ShopAsync<T>(this TestHub hub, Func<ISkinShopService, Task<T>> action)
    {
        return hub.RunAsync(services => action(services.GetRequiredService<ISkinShopService>()));
    }

    public static Task<List<Skin>> SkinsAsync(this TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<ISkinService>().ListAsync());
    }

    public static Task<List<SkinPurchase>> PurchasesAsync(this TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<SkinPurchase>().AsNoTracking().ToListAsync());
    }

    public static Task<List<SkinEquipment>> EquipmentAsync(this TestHub hub)
    {
        return hub.RunAsync(services => services.GetRequiredService<HubDbContext>().Set<SkinEquipment>().AsNoTracking().ToListAsync());
    }
}
