namespace L4D2PlayStats.Hub.Core.Modules.Championships.Helpers;

public static class ChampionshipGallery
{
    public static List<string> Parse(string? value)
    {
        return (value ?? string.Empty)
            .Split(AppConsts.ChampionshipGallerySeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToList();
    }

    public static string? Format(IEnumerable<string> images)
    {
        var joined = string.Join(AppConsts.ChampionshipGallerySeparator, images.Where(image => !string.IsNullOrWhiteSpace(image)).Select(image => image.Trim()).Distinct());

        return joined.Length == 0 ? null : joined;
    }
}
