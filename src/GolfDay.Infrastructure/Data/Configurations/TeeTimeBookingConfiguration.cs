using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class TeeTimeBookingConfiguration : IEntityTypeConfiguration<TeeTimeBooking>
{
    public void Configure(EntityTypeBuilder<TeeTimeBooking> builder)
    {
        builder.Property(b => b.Notes).HasMaxLength(500);
        builder.Property(b => b.CancellationReason).HasMaxLength(500);

        builder.HasIndex(b => new { b.TeeTimeSlotId, b.UserId }).IsUnique();

        builder.HasOne(b => b.TeeTimeSlot)
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.TeeTimeSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
