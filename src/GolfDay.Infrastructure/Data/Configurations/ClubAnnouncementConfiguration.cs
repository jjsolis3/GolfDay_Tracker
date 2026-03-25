using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class ClubAnnouncementConfiguration : IEntityTypeConfiguration<ClubAnnouncement>
{
    public void Configure(EntityTypeBuilder<ClubAnnouncement> builder)
    {
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Body).HasMaxLength(5000).IsRequired();

        builder.HasIndex(a => new { a.ClubId, a.IsPublished, a.IsPinned });

        builder.HasOne(a => a.Club)
            .WithMany()
            .HasForeignKey(a => a.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
