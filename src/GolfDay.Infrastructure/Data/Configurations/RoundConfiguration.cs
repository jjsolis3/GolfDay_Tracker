using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TeeBoxColor).HasMaxLength(20);
        builder.Property(r => r.Notes).HasMaxLength(2000);

        builder.HasOne(r => r.Event)
               .WithMany(e => e.Rounds)
               .HasForeignKey(r => r.EventId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Course)
               .WithMany(c => c.Rounds)
               .HasForeignKey(r => r.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User)
               .WithMany(u => u.Rounds)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.HoleScores)
               .WithOne(h => h.Round)
               .HasForeignKey(h => h.RoundId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
