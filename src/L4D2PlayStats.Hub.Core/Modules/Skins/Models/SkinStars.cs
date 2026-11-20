namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public record SkinStars(int MvpSiDamage, int MvpCommon)
{
    public static readonly SkinStars None = new(0, 0);

    public int Total => MvpSiDamage + MvpCommon;
}