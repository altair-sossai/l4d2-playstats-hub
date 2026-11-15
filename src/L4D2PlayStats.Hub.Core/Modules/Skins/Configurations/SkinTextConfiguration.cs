using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Configurations;

public class SkinTextConfiguration : IEntityTypeConfiguration<SkinText>
{
    public void Configure(EntityTypeBuilder<SkinText> builder)
    {
        builder.Property(e => e.SkinId).HasMaxLength(AppConsts.SkinIdMaxLength);
        builder.Property(e => e.Language).HasMaxLength(AppConsts.SkinLanguageMaxLength);
        builder.Property(e => e.Title).HasMaxLength(AppConsts.SkinTitleMaxLength);
        builder.HasIndex(e => new { e.SkinId, e.Language }).IsUnique();
    }
}
