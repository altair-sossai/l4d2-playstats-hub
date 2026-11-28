using System.Net;
using System.Text.Encodings.Web;
using L4D2PlayStats.Hub.Core.Data;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Enums;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace L4D2PlayStats.Hub.Tests.Web;

[TestClass]
public class NotificationPagesTests
{
    private const long Other = 76561198000000300;

    [TestMethod]
    public async Task TheBellCountsTheUnreadNotificationsAndOpeningOneGoesToItsPage()
    {
        using var factory = new HubWebFactory();
        using var player = factory.Client(HubWebFactory.Player);
        using var other = factory.Client(Other);
        using var visitor = factory.Client();

        await factory.CompleteSetupAsync();

        var visitorPage = await visitor.GetAsync("/notifications");

        Assert.AreEqual(HttpStatusCode.Unauthorized, visitorPage.StatusCode);
        Assert.DoesNotContain("nav-notifications", await visitor.GetStringAsync("/players"));

        var empty = await player.GetStringAsync("/notifications");

        Assert.Contains("nav-notifications", empty);
        Assert.DoesNotContain("nav-notifications-badge", empty);
        Assert.Contains("No notification yet.", empty);

        await SendAsync(factory, notifications => notifications.Send(HubWebFactory.Player, new NotificationMessage(NotificationTopic.Skins, "You received the skin {0} as a gift. It is already equipped.", "/skins", "AK-47 Gold")));

        var page = await player.GetStringAsync("/notifications");
        var id = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Notifications.AsNoTracking().Where(item => item.CommunityId == HubWebFactory.Player).Select(item => item.Id).SingleAsync());

        Assert.Contains("<span class=\"badge nav-notifications-badge\">1</span>", page);
        Assert.Contains("You received the skin AK-47 Gold as a gift. It is already equipped.", page);
        Assert.Contains("notifications-unread", page);
        Assert.Contains($"href=\"/notifications/{id}\"", page);
        Assert.Contains("data-confirm=\"Mark every notification as read?", page);

        Assert.AreEqual(HttpStatusCode.NotFound, (await other.GetAsync($"/notifications/{id}")).StatusCode);

        var opened = await player.GetAsync($"/notifications/{id}");

        Assert.AreEqual(HttpStatusCode.Redirect, opened.StatusCode);
        Assert.AreEqual("/skins", opened.Headers.Location!.OriginalString);

        var read = await player.GetStringAsync("/notifications");

        Assert.DoesNotContain("nav-notifications-badge", read);
        Assert.DoesNotContain("notifications-unread", read);
    }

    [TestMethod]
    public async Task MarkAllAsReadClearsTheCounterAndAnOutsideAddressIsNeverFollowed()
    {
        using var factory = new HubWebFactory();
        using var player = factory.Client(HubWebFactory.Player);
        using var admin = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await SendAsync(factory, notifications =>
        {
            notifications.SendEveryone(new NotificationMessage(NotificationTopic.Seasons, "A new season started: the ranking starts again from zero.", "https://example.com/"));
            notifications.SendEveryone(new NotificationMessage(NotificationTopic.Seasons, "A new season started: the ranking starts again from zero.", "/"));
            notifications.SendAdministrators(new NotificationMessage(NotificationTopic.Backups, "The automatic backup failed. Open the backup page to see why.", "/admin/backup"));
        });

        Assert.Contains("<span class=\"badge nav-notifications-badge\">2</span>", await player.GetStringAsync("/notifications"));
        Assert.Contains("<span class=\"badge nav-notifications-badge\">4</span>", await admin.GetStringAsync("/notifications"));
        Assert.DoesNotContain("The automatic backup failed.", await player.GetStringAsync("/notifications"));

        var outside = await factory.RunAsync(services => services.GetRequiredService<HubDbContext>().Notifications.AsNoTracking().Where(item => item.Url == "https://example.com/").Select(item => item.Id).SingleAsync());

        Assert.AreEqual("/notifications", (await player.GetAsync($"/notifications/{outside}")).Headers.Location!.OriginalString);

        Assert.AreEqual(HttpStatusCode.BadRequest, (await player.PostAsync("/notifications/read", new FormUrlEncodedContent(new Dictionary<string, string>()))).StatusCode);

        var marked = await HubWebFactory.PostFormAsync(player, "/notifications", "/notifications/read", new Dictionary<string, string> { ["returnUrl"] = "/players" });

        Assert.AreEqual(HttpStatusCode.Redirect, marked.StatusCode);
        Assert.AreEqual("/players", marked.Headers.Location!.OriginalString);
        Assert.DoesNotContain("nav-notifications-badge", await player.GetStringAsync("/notifications"));
        Assert.Contains("<span class=\"badge nav-notifications-badge\">4</span>", await admin.GetStringAsync("/notifications"));
    }

    [TestMethod]
    public async Task TheValuesThatArePlayersShowTheirNamesInTheLanguageOfTheVisitor()
    {
        using var factory = new HubWebFactory();
        using var admin = factory.Client(HubWebFactory.Administrator);

        await factory.CompleteSetupAsync();
        await SendAsync(factory, notifications => notifications.SendAdministrators(new NotificationMessage(NotificationTopic.Championships, "{0} asked for a place in {1}.", "/admin/championships/1?tab=participants", HubWebFactory.Administrator, "Copa")));

        admin.DefaultRequestHeaders.Add("Cookie", ".AspNetCore.Culture=c%3Dpt-BR%7Cuic%3Dpt-BR");

        var page = await admin.GetStringAsync("/notifications");

        Assert.Contains("Admin pediu uma vaga em Copa.", page);
        Assert.Contains(HtmlEncoder.Default.Encode("Agora você é administrador do site."), page);
    }

    private static Task SendAsync(HubWebFactory factory, Action<INotificationService> send)
    {
        return factory.RunAsync(async services =>
        {
            send(services.GetRequiredService<INotificationService>());
            await services.GetRequiredService<HubDbContext>().SaveChangesAsync();
        });
    }
}
