using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes.Configurations;

public class GameConfigurationConfiguration : IEntityTypeConfiguration<GameConfiguration>
{
    public void Configure(EntityTypeBuilder<GameConfiguration> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ConfigurationNameMaxLength);
        builder.HasIndex(e => new { e.Name, e.TeamSize }).IsUnique();
    }
}