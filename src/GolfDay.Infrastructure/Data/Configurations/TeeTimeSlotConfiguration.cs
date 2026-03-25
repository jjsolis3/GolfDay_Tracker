using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class TeeTimeSlotConfiguration : IEntityTypeConfiguration<TeeTimeSlot>
{
    public void Configure(EntityTypeBuilder<TeeTimeSlot> builder)
    {
        builder.Property(t => t.Notes).HasMaxLength(500);

        builder.HasIndex(t => new { t.ClubId, t.SlotDate, t.StartTime });

        builder.HasOne(t => t.Club)
            .WithMany()
            .HasForeignKey(t => t.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Course)
            .WithMany()
            .HasForeignKey(t => t.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
