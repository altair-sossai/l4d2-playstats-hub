using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Configurations;

public class SkinPurchaseConfiguration : IEntityTypeConfiguration<SkinPurchase>
{
    public void Configure(EntityTypeBuilder<SkinPurchase> builder)
    {
        builder.Property(e => e.SkinId).HasMaxLength(AppConsts.SkinIdMaxLength);
        builder.Property(e => e.PlayerName).HasMaxLength(AppConsts.SkinNameMaxLength);
        builder.Property(e => e.GiftedByName).HasMaxLength(AppConsts.SkinNameMaxLength);
        builder.HasIndex(e => new { e.CommunityId, e.SkinId }).IsUnique();
        builder.HasIndex(e => e.PurchasedAtUtc);
        builder.HasOne(e => e.Skin).WithMany().HasForeignKey(e => e.SkinId).OnDelete(DeleteBehavior.Restrict);
    }
}