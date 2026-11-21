using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Configurations;

public class ForcedNameConfiguration : IEntityTypeConfiguration<ForcedName>
{
    public void Configure(EntityTypeBuilder<ForcedName> builder)
    {
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ForcedNameMaxLength).IsRequired();
    }
}
