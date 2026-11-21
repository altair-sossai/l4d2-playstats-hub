using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Players.Configurations;

public class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.HasKey(e => e.CommunityId);
        builder.Property(e => e.CommunityId).ValueGeneratedNever();
        builder.Property(e => e.Name).HasMaxLength(AppConsts.PlayerNameColumnMaxLength);
        builder.Property(e => e.MostUsedName).HasMaxLength(AppConsts.PlayerNameColumnMaxLength);
        builder.Property(e => e.DisplayName).HasMaxLength(AppConsts.PlayerNameColumnMaxLength);
        builder.Property(e => e.ForcedName).HasMaxLength(AppConsts.ForcedNameMaxLength);
        builder.Property(e => e.SteamName).HasMaxLength(AppConsts.PlayerNameColumnMaxLength);
        builder.HasIndex(e => e.DisplayName);
        builder.HasIndex(e => e.ForcedName);
    }
}
