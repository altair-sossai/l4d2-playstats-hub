using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipDateProposalConfiguration : IEntityTypeConfiguration<ChampionshipDateProposal>
{
    public void Configure(EntityTypeBuilder<ChampionshipDateProposal> builder)
    {
        builder.Ignore(e => e.Confirmed);
        builder.Ignore(e => e.Refused);
    }
}
