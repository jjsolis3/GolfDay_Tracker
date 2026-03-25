using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class SponsorConfiguration : IEntityTypeConfiguration<Sponsor>
{
    public void Configure(EntityTypeBuilder<Sponsor> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.LogoUrl).HasMaxLength(500);
        builder.Property(s => s.Website).HasMaxLength(300);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.ContactName).HasMaxLength(100);
        builder.Property(s => s.ContactEmail).HasMaxLength(200);

        builder.HasIndex(s => new { s.ClubId, s.IsActive });

        builder.HasOne(s => s.Club)
            .WithMany()
            .HasForeignKey(s => s.ClubId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
