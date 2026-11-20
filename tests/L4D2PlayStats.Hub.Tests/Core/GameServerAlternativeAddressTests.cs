using L4D2PlayStats.Hub.Core.Modules.Servers.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Core;

[TestClass]
public class GameServerAlternativeAddressTests
{
    [TestMethod]
    public async Task AServerKeepsAnAlternativeAddressBesideItsAddress()
    {
        using var hub = new TestHub();

        var credentials = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Zone", "200.100.50.1:27015"));

        await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().SetAlternativeAddressAsync(credentials.Server.Id, "  server.example.com  "));

        var server = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().FindAsync(credentials.Server.Id));

        Assert.AreEqual("200.100.50.1:27015", server!.Address);
        Assert.AreEqual("server.example.com", server.AlternativeAddress);
    }

    [TestMethod]
    public async Task ABlankAlternativeAddressIsRemoved()
    {
        using var hub = new TestHub();

        var credentials = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Zone", "200.100.50.1:27015"));

        await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().SetAlternativeAddressAsync(credentials.Server.Id, "server.example.com"));
        await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().SetAlternativeAddressAsync(credentials.Server.Id, "   "));

        var server = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().FindAsync(credentials.Server.Id));
        var unknown = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().SetAlternativeAddressAsync(credentials.Server.Id + 100, "server.example.com"));

        Assert.IsNull(server!.AlternativeAddress);
        Assert.IsNull(unknown);
    }

    [TestMethod]
    public async Task ChangingTheOtherDetailsKeepsTheAlternativeAddress()
    {
        using var hub = new TestHub();

        var credentials = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().CreateAsync("Zone", "200.100.50.1:27015"));

        await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().SetAlternativeAddressAsync(credentials.Server.Id, "server.example.com"));
        await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().UpdateAsync(credentials.Server.Id, "Zone 2", "200.100.50.2:27015", true));

        var server = await hub.RunAsync(services => services.GetRequiredService<IGameServerService>().FindAsync(credentials.Server.Id));

        Assert.AreEqual("Zone 2", server!.Name);
        Assert.AreEqual("server.example.com", server.AlternativeAddress);
    }
}
