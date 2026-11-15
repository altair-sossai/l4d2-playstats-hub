using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Configurations;

public class SkinEquipmentConfiguration : IEntityTypeConfiguration<SkinEquipment>
{
    public void Configure(EntityTypeBuilder<SkinEquipment> builder)
    {
        builder.Property(e => e.Weapon).HasMaxLength(AppConsts.SkinWeaponMaxLength);
        builder.Property(e => e.SkinId).HasMaxLength(AppConsts.SkinIdMaxLength);
        builder.Property(e => e.Source).HasMaxLength(AppConsts.SkinSourceMaxLength);
        builder.HasIndex(e => new { e.CommunityId, e.Weapon }).IsUnique();
        builder.HasOne<Skin>().WithMany().HasForeignKey(e => e.SkinId).OnDelete(DeleteBehavior.Restrict);
    }
}
