using System.Globalization;

namespace L4D2PlayStats.Hub.Web.Helpers;

public static class QueryUrl
{
    public static string Build(string path, params (string Key, object? Value)[] parameters)
    {
        List<string> query = [.. parameters.Where(parameter => Included(parameter.Key, parameter.Value)).Select(parameter => $"{parameter.Key}={Format(parameter.Value!)}")];

        if (query.Count == 0)
            return path;

        var separator = path.Contains('?') ? '&' : '?';

        return $"{path}{separator}{string.Join('&', query)}";
    }

    private static bool Included(string key, object? value)
    {
        return value switch
        {
            null => false,
            string text => !string.IsNullOrWhiteSpace(text),
            int number when key.EndsWith(AppConsts.AdminPageQuery, StringComparison.OrdinalIgnoreCase) => number > 1,
            _ => true
        };
    }

    private static string Format(object value)
    {
        return value switch
        {
            string text => Uri.EscapeDataString(text.Trim()),
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
