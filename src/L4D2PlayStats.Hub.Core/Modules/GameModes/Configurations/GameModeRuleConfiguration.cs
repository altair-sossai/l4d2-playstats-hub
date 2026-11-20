using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.GameModes.Configurations;

public class GameModeRuleConfiguration : IEntityTypeConfiguration<GameModeRule>
{
    public void Configure(EntityTypeBuilder<GameModeRule> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.GameModeNameMaxLength);
        builder.Property(e => e.ConfigurationPattern).HasMaxLength(AppConsts.ConfigurationNameMaxLength);
    }
}
