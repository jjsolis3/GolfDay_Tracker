using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class LeagueMatch : BaseEntity
{
    public int LeagueSeasonId { get; set; }
    public string Player1Id { get; set; } = string.Empty;
    public string Player2Id { get; set; } = string.Empty;
    public int? CourseId { get; set; }
    public int? EventId { get; set; }
    public DateTime ScheduledDeadline { get; set; }
    public DateTime? ProposedDate { get; set; }
    public string? ProposedBy { get; set; }
    public DateTime? PlayedDate { get; set; }
    public MatchStatus Status { get; set; } = MatchStatus.Scheduled;
    public string? WinnerId { get; set; }
    public bool IsDraw { get; set; } = false;
    public int? Player1GrossScore { get; set; }
    public int? Player2GrossScore { get; set; }
    public int? Player1NetScore { get; set; }
    public int? Player2NetScore { get; set; }
    public int? MatchPlayResult { get; set; }
    public string? ResultDescription { get; set; }
    public string? Notes { get; set; }
    public int RoundNumber { get; set; } = 1;

    // Navigation
    public LeagueSeason LeagueSeason { get; set; } = null!;
    public ApplicationUser Player1 { get; set; } = null!;
    public ApplicationUser Player2 { get; set; } = null!;
    public GolfCourse? Course { get; set; }
    public GolfEvent? Event { get; set; }
}
