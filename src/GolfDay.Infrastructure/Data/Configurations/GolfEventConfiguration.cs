using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class GolfEventConfiguration : IEntityTypeConfiguration<GolfEvent>
{
    public void Configure(EntityTypeBuilder<GolfEvent> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Format).HasMaxLength(100);
        builder.Property(e => e.TeeBoxColor).HasMaxLength(20);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.Club)
               .WithMany(c => c.Events)
               .HasForeignKey(e => e.ClubId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Course)
               .WithMany(c => c.Events)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Tournament)
               .WithMany(t => t.Rounds)
               .HasForeignKey(e => e.TournamentId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.LeagueSeason)
               .WithMany(l => l.Events)
               .HasForeignKey(e => e.LeagueSeasonId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
