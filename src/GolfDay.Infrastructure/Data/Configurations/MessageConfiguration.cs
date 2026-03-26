using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.Body).IsRequired().HasMaxLength(2000);

        builder.HasOne(m => m.FromUser)
            .WithMany()
            .HasForeignKey(m => m.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ToUser)
            .WithMany()
            .HasForeignKey(m => m.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.LeagueMatch)
            .WithMany()
            .HasForeignKey(m => m.LeagueMatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
