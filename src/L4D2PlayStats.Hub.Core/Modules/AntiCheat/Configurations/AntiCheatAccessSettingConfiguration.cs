using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Configurations;

public class AntiCheatAccessSettingConfiguration : IEntityTypeConfiguration<AntiCheatAccessSetting>
{
    public void Configure(EntityTypeBuilder<AntiCheatAccessSetting> builder)
    {
        builder.ToTable(AppConsts.AntiCheatAccessSettingsTable);
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.UpdatedByName).HasMaxLength(AppConsts.AntiCheatAccessNameMaxLength);
    }
}