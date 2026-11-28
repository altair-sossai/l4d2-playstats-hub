using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipGroupConfiguration : IEntityTypeConfiguration<ChampionshipGroup>
{
    public void Configure(EntityTypeBuilder<ChampionshipGroup> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ChampionshipGroupNameMaxLength).IsRequired();
        builder.HasMany(e => e.Teams).WithOne().HasForeignKey(e => e.GroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
