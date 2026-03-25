using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class TeeTimeBooking : BaseEntity
{
    public int TeeTimeSlotId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TeeTimeBookingStatus Status { get; set; } = TeeTimeBookingStatus.Confirmed;
    public int NumberOfPlayers { get; set; } = 1;
    public string? Notes { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Navigation
    public TeeTimeSlot TeeTimeSlot { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
