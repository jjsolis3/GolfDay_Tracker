using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class CourseConditionReport : BaseEntity
{
    public int CourseId { get; set; }
    public int ClubId { get; set; }
    public string ReportedByUserId { get; set; } = string.Empty;

    public FairwayCondition FairwayCondition { get; set; } = FairwayCondition.Good;
    public GreenSpeed GreenSpeed { get; set; } = GreenSpeed.Normal;
    public string? PinPositions { get; set; }
    public string? Notes { get; set; }

    /// <summary>Set by a manager — shown prominently on the event page.</summary>
    public bool IsOfficialReport { get; set; } = false;

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public GolfCourse Course { get; set; } = null!;
    public Club Club { get; set; } = null!;
    public ApplicationUser ReportedBy { get; set; } = null!;
}
