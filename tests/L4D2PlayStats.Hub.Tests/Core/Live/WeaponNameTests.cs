using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Extensions;

namespace L4D2PlayStats.Hub.Tests.Core.Live;

[TestClass]
public class WeaponNameTests
{
    [TestMethod]
    public void EveryWeaponHasTheNameOfItsIcon()
    {
        var missing = Enum.GetValues<Weapon>().Where(weapon => string.IsNullOrEmpty(weapon.WeaponName())).ToList();

        Assert.IsEmpty(missing);
        Assert.AreEqual("weapon_smg", Weapon.Smg.WeaponName());
        Assert.AreEqual("weapon_hunting_rifle", Weapon.HuntingRifle.WeaponName());
        Assert.AreEqual("Uzi", Weapon.Smg.Description());
    }

    [TestMethod]
    public void EveryMeleeWeaponButNoneHasTheNameOfItsIcon()
    {
        var missing = Enum.GetValues<MeleeWeapon>()
            .Where(weapon => weapon != MeleeWeapon.MeleeNone && string.IsNullOrEmpty(weapon.WeaponName()))
            .ToList();

        Assert.IsEmpty(missing);
        Assert.AreEqual(string.Empty, MeleeWeapon.MeleeNone.WeaponName());
        Assert.AreEqual("baseball_bat", MeleeWeapon.BaseballBat.WeaponName());
        Assert.AreEqual("Baseball Bat", MeleeWeapon.BaseballBat.Description());
    }
}
