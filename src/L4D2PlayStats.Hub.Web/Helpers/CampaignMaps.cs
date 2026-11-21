namespace L4D2PlayStats.Hub.Web.Helpers;

public static class CampaignMaps
{
    public static string[] Parse(string? maps)
    {
        return (maps ?? string.Empty).Split(['\n', '\r', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
