using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Enums;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Extensions;
using L4D2PlayStats.Hub.Core.Modules.AntiCheat.Models;
using L4D2PlayStats.Hub.Core.Modules.Settings.Models;

namespace L4D2PlayStats.Hub.Web.Models.AntiCheat;

public class AntiCheatAdminModel(AntiCheatSettings settings, AntiCheatAccessSnapshot access, AntiCheatAccessList shownList, PagedListModel<AntiCheatAccessPlayerResult> players, string? query, TimeZoneInfo timeZone, IReadOnlyList<long> listedIds, int otherCount)
{
    public AntiCheatSettings Settings { get; } = settings;
    public AntiCheatAccessSnapshot Access { get; } = access;
    public AntiCheatAccessMode Mode => Access.Mode;
    public AntiCheatAccessList ActiveList => Access.Mode.ListOf();
    public AntiCheatAccessList ShownList { get; } = shownList;
    public AntiCheatAccessList OtherList => ShownList.Other();
    public bool ShowingActiveList => ShownList == ActiveList;
    public PagedListModel<AntiCheatAccessPlayerResult> Players { get; } = players;
    public string? Query { get; } = query;
    public TimeZoneInfo TimeZone { get; } = timeZone;
    public IReadOnlyList<long> ListedIds { get; } = listedIds;
    public int ShownCount => ListedIds.Count;
    public int OtherCount { get; } = otherCount;
    public string ShownListName => ShownList.ToName();
    public string OtherListName => OtherList.ToName();
}
