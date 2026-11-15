using System.Globalization;
using L4D2PlayStats.Hub.Core.Modules.Skins;

namespace L4D2PlayStats.Hub.Web.Models.Skins;

public class SkinPurchaseRow(SkinPurchase purchase, Skin? skin, string playerName, string? giftedByName)
{
    public SkinPurchase Purchase { get; } = purchase;
    public Skin? Skin { get; } = skin;
    public string PlayerName { get; } = playerName;
    public string? GiftedByName { get; } = giftedByName;

    public string SkinTitle(CultureInfo culture)
    {
        return Skin?.Title(culture) ?? Purchase.SkinId;
    }

    public bool Matches(string? query, CultureInfo culture)
    {
        if (string.IsNullOrWhiteSpace(query))
            return true;

        var term = query.Trim();

        return PlayerName.Contains(term, StringComparison.OrdinalIgnoreCase)
               || Purchase.CommunityId.ToString().Contains(term, StringComparison.Ordinal)
               || Purchase.SkinId.Contains(term, StringComparison.OrdinalIgnoreCase)
               || SkinTitle(culture).Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
