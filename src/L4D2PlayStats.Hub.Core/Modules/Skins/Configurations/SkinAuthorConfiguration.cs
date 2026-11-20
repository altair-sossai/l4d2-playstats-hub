using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Skins.Configurations;

public class SkinAuthorConfiguration : IEntityTypeConfiguration<SkinAuthor>
{
    public void Configure(EntityTypeBuilder<SkinAuthor> builder)
    {
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(AppConsts.SkinNameMaxLength);
        builder.HasIndex(e => e.SyncedAtUtc);
    }
}