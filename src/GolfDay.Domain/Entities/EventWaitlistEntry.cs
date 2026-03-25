using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class EventWaitlistEntry : BaseEntity
{
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Position { get; set; }
    public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;
    public DateTime? PromotedAt { get; set; }
    public DateTime? NotifiedAt { get; set; }

    // Navigation
    public GolfEvent Event { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
