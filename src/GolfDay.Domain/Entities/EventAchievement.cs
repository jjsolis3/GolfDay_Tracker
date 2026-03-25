using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class EventAchievement : BaseEntity
{
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AchievementType Type { get; set; }
    public string? Description { get; set; }
    public int? ValueYards { get; set; }
    public int? ValueFeet { get; set; }
    public int? ValueInches { get; set; }
    public int? HoleNumber { get; set; }
    public bool IsVerified { get; set; } = false;
    public string? VerifiedBy { get; set; }

    // Navigation
    public GolfEvent Event { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
