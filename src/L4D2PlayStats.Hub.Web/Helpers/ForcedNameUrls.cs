namespace L4D2PlayStats.Hub.Web.Helpers;

public static class ForcedNameUrls
{
    public static string Page(string? search, int page)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
            query.Add($"q={Uri.EscapeDataString(search.Trim())}");

        if (page > 1)
            query.Add($"page={page}");

        return query.Count == 0 ? AppConsts.ForcedNamesUrl : $"{AppConsts.ForcedNamesUrl}?{string.Join("&", query)}";
    }
}
