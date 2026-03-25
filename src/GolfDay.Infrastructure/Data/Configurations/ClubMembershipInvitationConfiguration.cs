using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class ClubMembershipInvitationConfiguration : IEntityTypeConfiguration<ClubMembershipInvitation>
{
    public void Configure(EntityTypeBuilder<ClubMembershipInvitation> builder)
    {
        builder.HasIndex(i => i.Token).IsUnique();
        builder.Property(i => i.Token).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Email).HasMaxLength(256);
        builder.Property(i => i.Label).HasMaxLength(200);

        builder.HasOne(i => i.Club)
            .WithMany()
            .HasForeignKey(i => i.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.InvitedBy)
            .WithMany()
            .HasForeignKey(i => i.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
