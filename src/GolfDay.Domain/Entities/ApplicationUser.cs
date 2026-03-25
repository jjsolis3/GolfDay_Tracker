using Microsoft.AspNetCore.Identity;

namespace GolfDay.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? ProfilePictureUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public double? HandicapIndex { get; set; }
    public string? GhinNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation
    public ICollection<ClubMembership> ClubMemberships { get; set; } = new List<ClubMembership>();
    public ICollection<EventParticipant> EventParticipations { get; set; } = new List<EventParticipant>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    public ICollection<TournamentEntry> TournamentEntries { get; set; } = new List<TournamentEntry>();
    public ICollection<LeagueEntry> LeagueEntries { get; set; } = new List<LeagueEntry>();
    public ICollection<PlayerSeasonStats> SeasonStats { get; set; } = new List<PlayerSeasonStats>();
}
