using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class TournamentEntry : BaseEntity
{
    public int TournamentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public EntryStatus Status { get; set; } = EntryStatus.Registered;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public double? HandicapAtEntry { get; set; }
    public int? TotalGrossScore { get; set; }
    public int? TotalNetScore { get; set; }
    public int? TotalStablefordPoints { get; set; }
    public int? FinalPosition { get; set; }
    public string? Prize { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Tournament Tournament { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
