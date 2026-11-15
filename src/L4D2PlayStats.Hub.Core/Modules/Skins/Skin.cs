using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;
using System.Globalization;

namespace L4D2PlayStats.Hub.Core.Modules.Skins;

public class Skin
{
    public string Id { get; set; } = null!;
    public string Weapon { get; set; } = null!;
    public SkinRarity Rarity { get; set; }
    public int DefaultPrice { get; set; }
    public string? WorkshopId { get; set; }
    public string? AuthorName { get; set; }
    public long? AuthorCommunityId { get; set; }
    public string? OriginalTitle { get; set; }
    public string? OriginalDescription { get; set; }
    public int Shots { get; set; }
    public bool Available { get; set; } = true;
    public DateTime ImportedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<SkinText> Texts { get; set; } = [];
    public SkinPriceOverride? PriceOverride { get; set; }

    public int Price => PriceOverride?.Price ?? DefaultPrice;
    public bool HasCustomPrice => PriceOverride != null;
    public bool HasShots => Shots > 0;
    public string Command => $"{AppConsts.SkinCommand} {Id}";

    public string Title(CultureInfo culture)
    {
        return Localized(culture, text => text.Title) ?? Id;
    }

    public string? Description(CultureInfo culture)
    {
        return Localized(culture, text => text.Description);
    }

    private string? Localized(CultureInfo culture, Func<SkinText, string?> selector)
    {
        string[] languages = [culture.TwoLetterISOLanguageName, AppConsts.SkinDefaultLanguage];

        var preferred = languages
            .SelectMany(language => Texts.Where(text => text.Language == language))
            .Select(selector)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return preferred ?? Texts.Select(selector).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
