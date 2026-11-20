using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Administrators.Configurations;

public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
{
    public void Configure(EntityTypeBuilder<Administrator> builder)
    {
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(AppConsts.PlayerNameColumnMaxLength);
    }
}