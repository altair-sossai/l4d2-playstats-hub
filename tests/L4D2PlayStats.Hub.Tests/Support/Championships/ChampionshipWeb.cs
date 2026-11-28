using L4D2PlayStats.Hub.Core.Modules.Championships;
using L4D2PlayStats.Hub.Core.Modules.Championships.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Support.Championships;

public static class ChampionshipWeb
{
    public static async Task<HubWebFactory> FactoryAsync()
    {
        var factory = new HubWebFactory();

        await factory.CompleteSetupAsync();
        await factory.SaveAsync(new ModuleSettings { Ranking = true, LiveServer = true, Championships = true });

        return factory;
    }

    public static Task<Championship> LoadAsync(HubWebFactory factory, int id)
    {
        return factory.RunAsync(async services => (await services.GetRequiredService<IChampionshipLoadService>().LoadAsync(id, false))!);
    }
}
