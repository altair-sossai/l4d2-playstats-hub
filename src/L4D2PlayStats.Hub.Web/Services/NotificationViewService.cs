using L4D2PlayStats.Hub.Core.Modules.Notifications.Models;
using L4D2PlayStats.Hub.Core.Modules.Notifications.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Players.Services.Contracts;
using L4D2PlayStats.Hub.Core.Modules.Settings.Services.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Authentication.Contracts;
using L4D2PlayStats.Hub.Web.Infrastructure.Localization;
using L4D2PlayStats.Hub.Web.Models;
using L4D2PlayStats.Hub.Web.Models.Notifications;
using L4D2PlayStats.Hub.Web.Services.Contracts;
using Microsoft.Extensions.Localization;
using CoreConsts = L4D2PlayStats.Hub.Core.AppConsts;

namespace L4D2PlayStats.Hub.Web.Services;

public class NotificationViewService(INotificationService notifications, IPlayerNameService playerNames, ISiteClock siteClock, ICurrentUser currentUser, IStringLocalizer<SharedResource> L) : INotificationViewService
{
    public async Task<NotificationBellModel?> BellAsync(CancellationToken cancellationToken = default)
    {
        if (currentUser.CommunityId is not { } me)
            return null;

        var administrator = await currentUser.IsAdministratorAsync();
        var latest = await notifications.LatestAsync(me, administrator, AppConsts.NotificationBellSize, cancellationToken);

        return new NotificationBellModel(await ViewsAsync(latest, cancellationToken), await notifications.UnreadAsync(me, administrator, cancellationToken), await siteClock.ZoneAsync(cancellationToken));
    }

    public async Task<NotificationsIndexModel?> PageAsync(int page, CancellationToken cancellationToken = default)
    {
        if (currentUser.CommunityId is not { } me)
            return null;

        var administrator = await currentUser.IsAdministratorAsync();
        var rows = await notifications.PageAsync(me, administrator, page, AppConsts.NotificationsPageSize, cancellationToken);
        var views = await ViewsAsync(rows.Items.ToList(), cancellationToken);
        var lookup = views.ToDictionary(view => view.Id);

        return new NotificationsIndexModel(new PagedListModel<NotificationView>(rows.Map(item => lookup[item.Id])), await notifications.UnreadAsync(me, administrator, cancellationToken), await siteClock.ZoneAsync(cancellationToken));
    }

    private async Task<List<NotificationView>> ViewsAsync(IReadOnlyCollection<NotificationItem> items, CancellationToken cancellationToken)
    {
        var ids = items.SelectMany(item => item.Values).Select(value => long.TryParse(value, out var id) ? id : 0).Where(id => id >= CoreConsts.SteamCommunityIdBase).Distinct().ToList();
        var names = ids.Count == 0 ? [] : await playerNames.NamesWithAdministratorsAsync(ids, cancellationToken);

        return items.Select(item =>
        {
            var values = item.Values.Select(value => (object)(long.TryParse(value, out var id) && names.TryGetValue(id, out var name) ? name : value)).ToArray();

            return new NotificationView(item.Id, item.Topic, L[item.Text, values].Value, item.Url, item.CreatedAtUtc, item.Read);
        }).ToList();
    }
}
