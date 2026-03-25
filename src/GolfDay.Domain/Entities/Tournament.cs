using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class Tournament : BaseEntity
{
    public int ClubId { get; set; }
    public int? CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TournamentFormat Format { get; set; } = TournamentFormat.StrokePlay;
    public TournamentStatus Status { get; set; } = TournamentStatus.Registration;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RegistrationDeadline { get; set; }
    public int MaxParticipants { get; set; } = 100;
    public int NumberOfRounds { get; set; } = 1;
    public bool IsPublic { get; set; } = false;
    public bool UseHandicaps { get; set; } = true;
    public string? Rules { get; set; }
    public string? Prizes { get; set; }
    public string? Sponsors { get; set; }
    public string? Notes { get; set; }
    public string? BannerImageUrl { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public GolfCourse? Course { get; set; }
    public ICollection<TournamentEntry> Entries { get; set; } = new List<TournamentEntry>();
    public ICollection<GolfEvent> Rounds { get; set; } = new List<GolfEvent>();
}
