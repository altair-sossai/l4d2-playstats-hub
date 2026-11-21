namespace L4D2PlayStats.Hub.Core.Infrastructure.Helpers;

public static class Text
{
    public static string? Clip(string? value, int maxLength)
    {
        var text = value?.Trim();

        if (string.IsNullOrEmpty(text))
            return null;

        return text.Length > maxLength ? text[..maxLength] : text;
    }
}
