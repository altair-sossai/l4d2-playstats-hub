using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Configurations;

public class SkinConfiguration : IEntityTypeConfiguration<Skin>
{
    public void Configure(EntityTypeBuilder<Skin> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasMaxLength(AppConsts.SkinIdMaxLength).ValueGeneratedNever();
        builder.Property(e => e.Weapon).HasMaxLength(AppConsts.SkinWeaponMaxLength);
        builder.Property(e => e.WorkshopId).HasMaxLength(AppConsts.SkinWorkshopIdMaxLength);
        builder.Property(e => e.AuthorName).HasMaxLength(AppConsts.SkinNameMaxLength);
        builder.Property(e => e.OriginalTitle).HasMaxLength(AppConsts.SkinTitleMaxLength);
        builder.HasIndex(e => e.Weapon);
        builder.HasMany(e => e.Texts).WithOne().HasForeignKey(e => e.SkinId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.PriceOverride).WithOne().HasForeignKey<SkinPriceOverride>(e => e.SkinId).OnDelete(DeleteBehavior.Cascade);
    }
}
