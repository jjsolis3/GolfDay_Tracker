using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class EventParticipant : BaseEntity
{
    public int EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool CheckedIn { get; set; } = false;
    public DateTime? CheckedInAt { get; set; }
    public string? TeeTime { get; set; }
    public int? StartingHole { get; set; }
    public int? GroupNumber { get; set; }
    public string? Cart { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public GolfEvent Event { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
