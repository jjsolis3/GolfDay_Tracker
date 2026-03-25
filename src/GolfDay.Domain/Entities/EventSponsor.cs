using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

/// <summary>Links a sponsor to a specific event or tournament.</summary>
public class EventSponsor : BaseEntity
{
    public int SponsorId { get; set; }
    public int? EventId { get; set; }
    public int? TournamentId { get; set; }
    public int? LeagueSeasonId { get; set; }
    public int DisplayOrder { get; set; } = 0;

    // Navigation
    public Sponsor Sponsor { get; set; } = null!;
    public GolfEvent? Event { get; set; }
    public Tournament? Tournament { get; set; }
    public LeagueSeason? LeagueSeason { get; set; }
}
