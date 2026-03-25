using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class ClubJoinRequestConfiguration : IEntityTypeConfiguration<ClubJoinRequest>
{
    public void Configure(EntityTypeBuilder<ClubJoinRequest> builder)
    {
        builder.Property(r => r.ApplicantMessage).HasMaxLength(1000);
        builder.Property(r => r.ReviewNotes).HasMaxLength(500);

        builder.HasOne(r => r.Club)
            .WithMany()
            .HasForeignKey(r => r.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedBy)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
