using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class Round : BaseEntity
{
    public int? EventId { get; set; }
    public int CourseId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int RoundNumber { get; set; } = 1;
    public RoundStatus Status { get; set; } = RoundStatus.NotStarted;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? GrossScore { get; set; }
    public int? NetScore { get; set; }
    public double? HandicapUsed { get; set; }
    public int? TotalPutts { get; set; }
    public int? FairwaysHit { get; set; }
    public int? TotalFairways { get; set; }
    public int? GreensInRegulation { get; set; }
    public int? TotalGreens { get; set; }
    public int? LongestDriveYards { get; set; }
    public int? HoleInOnes { get; set; }
    public int? Eagles { get; set; }
    public int? Birdies { get; set; }
    public int? Pars { get; set; }
    public int? Bogeys { get; set; }
    public int? DoubleBogeys { get; set; }
    public int? TriplePlusBogeys { get; set; }
    public string? TeeBoxColor { get; set; }
    public string? Notes { get; set; }
    public int? StablefordPoints { get; set; }

    // ── Scorecard Attestation ─────────────────────────────────────────────────
    public bool IsAttested { get; set; } = false;
    public string? AttestedByUserId { get; set; }
    public DateTime? AttestedAt { get; set; }
    public string? AttestationNotes { get; set; }

    // Navigation
    public GolfEvent? Event { get; set; }
    public GolfCourse Course { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<HoleScore> HoleScores { get; set; } = new List<HoleScore>();
}
