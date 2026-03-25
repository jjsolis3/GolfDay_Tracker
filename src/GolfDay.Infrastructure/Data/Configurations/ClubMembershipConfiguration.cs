using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class ClubMembershipConfiguration : IEntityTypeConfiguration<ClubMembership>
{
    public void Configure(EntityTypeBuilder<ClubMembership> builder)
    {
        builder.Property(m => m.MemberNumber).HasMaxLength(50);
        builder.Property(m => m.Notes).HasMaxLength(1000);
        builder.Property(m => m.PaymentNotes).HasMaxLength(500);
        builder.Property(m => m.AnnualDueAmount).HasColumnType("decimal(10,2)");
        builder.Property(m => m.LastPaymentAmount).HasColumnType("decimal(10,2)");
        builder.Property(m => m.TotalPaid).HasColumnType("decimal(10,2)");

        builder.HasOne(m => m.Club)
            .WithMany(c => c.Memberships)
            .HasForeignKey(m => m.ClubId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Invitation)
            .WithMany(i => i.ResultingMemberships)
            .HasForeignKey(m => m.InvitationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.JoinRequest)
            .WithMany()
            .HasForeignKey(m => m.JoinRequestId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
