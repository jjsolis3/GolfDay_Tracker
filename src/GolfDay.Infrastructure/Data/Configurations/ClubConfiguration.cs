using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class ClubConfiguration : IEntityTypeConfiguration<Club>
{
    public void Configure(EntityTypeBuilder<Club> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Slug).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.ContactEmail).HasMaxLength(200);
        builder.Property(c => c.ContactPhone).HasMaxLength(20);
        builder.Property(c => c.Address).HasMaxLength(300);
        builder.Property(c => c.City).HasMaxLength(100);
        builder.Property(c => c.State).HasMaxLength(50);
        builder.Property(c => c.ZipCode).HasMaxLength(20);
        builder.Property(c => c.Country).HasMaxLength(50);
        builder.Property(c => c.TimeZone).HasMaxLength(50);
    }
}
