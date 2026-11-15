namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class SkinWallet(int stars, int spent)
{
    public int Stars { get; } = stars;
    public int Spent { get; } = spent;
    public int Balance => Math.Max(0, Stars - Spent);
}
