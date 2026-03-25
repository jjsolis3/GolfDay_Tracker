using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class GolfEvent : BaseEntity
{
    public int ClubId { get; set; }
    public int? CourseId { get; set; }
    public int? TournamentId { get; set; }
    public int? LeagueSeasonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EventType EventType { get; set; } = EventType.DayEvent;
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    public DateTime EventDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Format { get; set; }
    public int MaxParticipants { get; set; } = 100;
    public bool IsPublic { get; set; } = false;
    public bool UseHandicaps { get; set; } = true;
    public string? Rules { get; set; }
    public string? Notes { get; set; }
    public string? TeeBoxColor { get; set; }
    public int? RoundNumber { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public GolfCourse? Course { get; set; }
    public Tournament? Tournament { get; set; }
    public LeagueSeason? LeagueSeason { get; set; }
    public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
    public ICollection<EventAchievement> Achievements { get; set; } = new List<EventAchievement>();
    public ICollection<LeagueMatch> LeagueMatches { get; set; } = new List<LeagueMatch>();
}
