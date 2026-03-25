using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class LeagueSeason : BaseEntity
{
    public int ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LeagueFormat Format { get; set; } = LeagueFormat.RoundRobin;
    public LeagueStatus Status { get; set; } = LeagueStatus.Registration;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public int MaxParticipants { get; set; } = 32;
    public int MatchDeadlineDays { get; set; } = 14;
    public int PointsForWin { get; set; } = 3;
    public int PointsForDraw { get; set; } = 1;
    public int PointsForLoss { get; set; } = 0;
    public bool UseHandicaps { get; set; } = true;
    public string? Rules { get; set; }
    public string? Prizes { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public ICollection<LeagueEntry> Entries { get; set; } = new List<LeagueEntry>();
    public ICollection<LeagueMatch> Matches { get; set; } = new List<LeagueMatch>();
    public ICollection<LeagueStanding> Standings { get; set; } = new List<LeagueStanding>();
    public ICollection<GolfEvent> Events { get; set; } = new List<GolfEvent>();
}
