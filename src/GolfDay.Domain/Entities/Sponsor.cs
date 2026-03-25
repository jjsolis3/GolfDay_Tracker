using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class Sponsor : BaseEntity
{
    public int ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public SponsorTier Tier { get; set; } = SponsorTier.Bronze;
    public bool IsActive { get; set; } = true;

    /// <summary>Year this sponsorship is active (null = all years).</summary>
    public int? SponsorshipYear { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public ICollection<EventSponsor> EventSponsors { get; set; } = new List<EventSponsor>();
}
