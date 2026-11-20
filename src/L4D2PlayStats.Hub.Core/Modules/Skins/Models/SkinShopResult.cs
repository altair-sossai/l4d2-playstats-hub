using L4D2PlayStats.Hub.Core.Modules.Skins.Enums;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Models;

public class SkinShopResult
{
    public SkinShopStatus Status { get; init; }
    public Skin? Skin { get; init; }
    public SkinWallet? Wallet { get; init; }
    public bool Succeeded => Status == SkinShopStatus.Success;

    public static SkinShopResult Of(SkinShopStatus status, Skin? skin = null, SkinWallet? wallet = null)
    {
        return new SkinShopResult { Status = status, Skin = skin, Wallet = wallet };
    }
}
