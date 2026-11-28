using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipTeamMemberConfiguration : IEntityTypeConfiguration<ChampionshipTeamMember>
{
    public void Configure(EntityTypeBuilder<ChampionshipTeamMember> builder)
    {
        builder.HasIndex(e => new { e.TeamId, e.CommunityId }).IsUnique();
        builder.HasIndex(e => e.CommunityId);
    }
}
