using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class CourseConditionReportConfiguration : IEntityTypeConfiguration<CourseConditionReport>
{
    public void Configure(EntityTypeBuilder<CourseConditionReport> builder)
    {
        builder.Property(r => r.PinPositions).HasMaxLength(500);
        builder.Property(r => r.Notes).HasMaxLength(1000);

        builder.HasIndex(r => new { r.CourseId, r.ReportedAt });

        builder.HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Club)
            .WithMany()
            .HasForeignKey(r => r.ClubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReportedBy)
            .WithMany()
            .HasForeignKey(r => r.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
