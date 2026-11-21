namespace L4D2PlayStats.Hub.Web.Helpers;

public static class CommunityDisplay
{
    public static string Rating(double rating)
    {
        return Display.Number((decimal)Math.Round(rating));
    }

    public static string Margin(double deviation)
    {
        return $"±{Display.Number((decimal)Math.Round(deviation))}";
    }

    public static string RatingWithMargin(double rating, double deviation)
    {
        return $"{Rating(rating)} {Margin(deviation)}";
    }
}
