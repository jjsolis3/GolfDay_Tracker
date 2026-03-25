using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class EventWaitlistEntryConfiguration : IEntityTypeConfiguration<EventWaitlistEntry>
{
    public void Configure(EntityTypeBuilder<EventWaitlistEntry> builder)
    {
        builder.HasIndex(w => new { w.EventId, w.UserId }).IsUnique();
        builder.HasIndex(w => new { w.EventId, w.Status, w.Position });

        builder.HasOne(w => w.Event)
            .WithMany()
            .HasForeignKey(w => w.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
