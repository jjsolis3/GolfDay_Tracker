using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class Club : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } = "USA";
    public string? TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = false;
    public DateTime? FoundedDate { get; set; }

    // Navigation
    public ICollection<ClubMembership> Memberships { get; set; } = new List<ClubMembership>();
    public ICollection<GolfCourse> Courses { get; set; } = new List<GolfCourse>();
    public ICollection<GolfEvent> Events { get; set; } = new List<GolfEvent>();
    public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    public ICollection<LeagueSeason> LeagueSeasons { get; set; } = new List<LeagueSeason>();
    public ICollection<PlayerSeasonStats> PlayerStats { get; set; } = new List<PlayerSeasonStats>();
}
