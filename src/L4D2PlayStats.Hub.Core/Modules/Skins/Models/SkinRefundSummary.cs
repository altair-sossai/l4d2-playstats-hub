namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class SkinRefundSummary
{
    public int Purchases { get; init; }
    public int Gifts { get; init; }
    public int Stars { get; init; }
    public int Players { get; init; }

    public static SkinRefundSummary Of(IReadOnlyCollection<SkinPurchase> refunded)
    {
        return new SkinRefundSummary
        {
            Purchases = refunded.Count(purchase => !purchase.Gift),
            Gifts = refunded.Count(purchase => purchase.Gift),
            Stars = refunded.Sum(purchase => purchase.Price),
            Players = refunded.Select(purchase => purchase.CommunityId).Distinct().Count()
        };
    }
}
