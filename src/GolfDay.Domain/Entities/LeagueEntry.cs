using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class LeagueEntry : BaseEntity
{
    public int LeagueSeasonId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public double? HandicapAtEntry { get; set; }
    public bool IsActive { get; set; } = true;
    public int? SeedNumber { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public LeagueSeason LeagueSeason { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
