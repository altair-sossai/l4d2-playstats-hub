namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class SkinWallet(SkinStars stars, int spent)
{
    public int MvpSiDamage { get; } = stars.MvpSiDamage;
    public int MvpCommon { get; } = stars.MvpCommon;
    public int Stars { get; } = stars.Total;
    public int Spent { get; } = spent;
    public int Balance => Math.Max(0, Stars - Spent);

    public SkinWallet After(int price)
    {
        return new SkinWallet(new SkinStars(MvpSiDamage, MvpCommon), Spent + price);
    }
}
