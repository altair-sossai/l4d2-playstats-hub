namespace L4D2PlayStats.Hub.Web.Models.Admin;

public record AdminTabsModel(IReadOnlyList<AdminTab> Tabs, string Active);
