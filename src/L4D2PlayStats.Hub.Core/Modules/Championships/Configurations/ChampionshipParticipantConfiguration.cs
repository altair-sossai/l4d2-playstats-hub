using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipParticipantConfiguration : IEntityTypeConfiguration<ChampionshipParticipant>
{
    public void Configure(EntityTypeBuilder<ChampionshipParticipant> builder)
    {
        builder.HasIndex(e => new { e.ChampionshipId, e.CommunityId }).IsUnique();
        builder.HasIndex(e => e.CommunityId);
    }
}
