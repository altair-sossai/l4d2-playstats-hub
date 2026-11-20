using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Matches.Configurations;

public class MatchPlayerConfiguration : IEntityTypeConfiguration<MatchPlayer>
{
    public void Configure(EntityTypeBuilder<MatchPlayer> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.PlayerNameColumnMaxLength);
        builder.Ignore(e => e.Teams);
        builder.HasIndex(e => e.CommunityId);
    }
}