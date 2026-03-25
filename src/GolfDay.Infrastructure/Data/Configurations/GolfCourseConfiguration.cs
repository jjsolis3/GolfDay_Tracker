using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class GolfCourseConfiguration : IEntityTypeConfiguration<GolfCourse>
{
    public void Configure(EntityTypeBuilder<GolfCourse> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.State).HasMaxLength(50);
        builder.Property(c => c.Country).HasMaxLength(50);
        builder.Property(c => c.CourseRating).HasPrecision(4, 1);
        builder.Property(c => c.SlopeRating).HasPrecision(5, 1);

        builder.HasOne(c => c.Club)
               .WithMany(cl => cl.Courses)
               .HasForeignKey(c => c.ClubId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Holes)
               .WithOne(h => h.Course)
               .HasForeignKey(h => h.CourseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
