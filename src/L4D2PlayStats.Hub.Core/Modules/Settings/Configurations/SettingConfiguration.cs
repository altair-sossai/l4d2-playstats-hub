using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Settings.Configurations;

public class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.HasKey(e => e.Key);
        builder.Property(e => e.Key).HasMaxLength(AppConsts.SettingKeyMaxLength);
    }
}
