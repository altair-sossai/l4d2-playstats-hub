using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Configurations;

public class SkinPriceOverrideConfiguration : IEntityTypeConfiguration<SkinPriceOverride>
{
    public void Configure(EntityTypeBuilder<SkinPriceOverride> builder)
    {
        builder.HasKey(e => e.SkinId);
        builder.Property(e => e.SkinId).HasMaxLength(AppConsts.SkinIdMaxLength).ValueGeneratedNever();
    }
}
