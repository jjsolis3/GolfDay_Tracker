using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class LeagueMatchConfiguration : IEntityTypeConfiguration<LeagueMatch>
{
    public void Configure(EntityTypeBuilder<LeagueMatch> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Notes).HasMaxLength(2000);
        builder.Property(m => m.ResultDescription).HasMaxLength(500);

        builder.HasOne(m => m.LeagueSeason)
               .WithMany(l => l.Matches)
               .HasForeignKey(m => m.LeagueSeasonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Player1)
               .WithMany()
               .HasForeignKey(m => m.Player1Id)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Player2)
               .WithMany()
               .HasForeignKey(m => m.Player2Id)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Course)
               .WithMany()
               .HasForeignKey(m => m.CourseId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Event)
               .WithMany(e => e.LeagueMatches)
               .HasForeignKey(m => m.EventId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
