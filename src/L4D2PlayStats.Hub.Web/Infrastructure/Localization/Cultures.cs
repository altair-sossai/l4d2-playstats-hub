using System.Globalization;

namespace L4D2PlayStats.Hub.Web.Infrastructure.Localization;

public static class Cultures
{
    public static List<CultureInfo> Supported()
    {
        return [.. AppConsts.SupportedCultures.Select(name => new CultureInfo(name))];
    }
}
