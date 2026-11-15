using L4D2PlayStats.Hub.Core.Modules.Administrators.Services;
using L4D2PlayStats.Hub.Core.Modules.Players.Helpers;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Extensions;

public static class HubCommands
{
    public static bool IsCommand(string[] args)
    {
        return args.Length > 0 && args[0] == AppConsts.AdminCommand;
    }

    public static async Task<int> RunCommandAsync(this WebApplication app, string[] args)
    {
        using var scope = app.Services.CreateScope();

        var administratorService = scope.ServiceProvider.GetRequiredService<IAdministratorService>();
        var action = args.Length > 1 ? args[1] : string.Empty;

        if (action == AppConsts.AdminListAction)
        {
            foreach (var administrator in await administratorService.ListAsync())
                Console.WriteLine($"{administrator.CommunityId}  {administrator.Name}");

            return 0;
        }

        if (action == AppConsts.AdminAddAction && args.Length > 2 && long.TryParse(args[2], out var communityId) && SteamIds.IsValid(communityId))
        {
            await administratorService.AddAsync(communityId, args.Length > 3 ? args[3] : null);
            Console.WriteLine($"{communityId} is now an administrator.");

            return 0;
        }

        Console.WriteLine("Usage:");
        Console.WriteLine("  admin list");
        Console.WriteLine("  admin add <SteamID64> [name]");

        return 1;
    }
}
