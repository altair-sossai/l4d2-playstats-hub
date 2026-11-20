using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.AntiCheat.Configurations;

public abstract class AntiCheatAccessPlayerConfiguration<T>(string table) : IEntityTypeConfiguration<T> where T : AntiCheatAccessPlayer
{
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(table);
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(AppConsts.AntiCheatAccessNameMaxLength);
        builder.Property(e => e.AddedByName).HasMaxLength(AppConsts.AntiCheatAccessNameMaxLength);
        builder.HasIndex(e => e.CreatedAtUtc);
    }
}