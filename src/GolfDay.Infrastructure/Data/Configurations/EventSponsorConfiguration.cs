using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GolfDay.Infrastructure.Data.Configurations;

public class EventSponsorConfiguration : IEntityTypeConfiguration<EventSponsor>
{
    public void Configure(EntityTypeBuilder<EventSponsor> builder)
    {
        builder.HasOne(es => es.Sponsor)
            .WithMany(s => s.EventSponsors)
            .HasForeignKey(es => es.SponsorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Event)
            .WithMany()
            .HasForeignKey(es => es.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(es => es.Tournament)
            .WithMany()
            .HasForeignKey(es => es.TournamentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(es => es.LeagueSeason)
            .WithMany()
            .HasForeignKey(es => es.LeagueSeasonId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
