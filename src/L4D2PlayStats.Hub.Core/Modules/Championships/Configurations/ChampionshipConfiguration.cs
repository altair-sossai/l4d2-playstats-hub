using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L4D2PlayStats.Hub.Core.Modules.Championships.Configurations;

public class ChampionshipConfiguration : IEntityTypeConfiguration<Championship>
{
    public void Configure(EntityTypeBuilder<Championship> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(AppConsts.ChampionshipNameMaxLength).IsRequired();
        builder.Property(e => e.Slug).HasMaxLength(AppConsts.ChampionshipSlugMaxLength).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(AppConsts.ChampionshipDescriptionMaxLength);
        builder.Property(e => e.Rules).HasMaxLength(AppConsts.ChampionshipRulesMaxLength);
        builder.Property(e => e.RulesPortuguese).HasMaxLength(AppConsts.ChampionshipRulesMaxLength);
        builder.Property(e => e.RulesSpanish).HasMaxLength(AppConsts.ChampionshipRulesMaxLength);
        builder.Property(e => e.StreamUrl).HasMaxLength(AppConsts.ChampionshipUrlMaxLength);
        builder.Property(e => e.VideosUrl).HasMaxLength(AppConsts.ChampionshipUrlMaxLength);
        builder.Property(e => e.BannerImage).HasMaxLength(AppConsts.ChampionshipImageMaxLength);
        builder.Property(e => e.LogoImage).HasMaxLength(AppConsts.ChampionshipImageMaxLength);
        builder.Property(e => e.GalleryImages).HasMaxLength(AppConsts.ChampionshipGalleryMaxLength);
        builder.Property(e => e.GameConfiguration).HasMaxLength(AppConsts.ChampionshipConfigurationMaxLength);
        builder.Property(e => e.CancelReason).HasMaxLength(AppConsts.ChampionshipReasonMaxLength);
        builder.HasIndex(e => e.Slug).IsUnique();
        builder.HasMany(e => e.Participants).WithOne().HasForeignKey(e => e.ChampionshipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Teams).WithOne().HasForeignKey(e => e.ChampionshipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Stages).WithOne().HasForeignKey(e => e.ChampionshipId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.DraftPicks).WithOne().HasForeignKey(e => e.ChampionshipId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(e => e.IsPublic);
        builder.Ignore(e => e.IsOver);
        builder.Ignore(e => e.Gallery);
        builder.Ignore(e => e.Images);
    }
}
