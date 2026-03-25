using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class TeeTimeSlot : BaseEntity
{
    public int ClubId { get; set; }
    public int CourseId { get; set; }

    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }

    public int MaxPlayers { get; set; } = 4;
    public bool IsActive { get; set; } = true;
    public bool IsBlocked { get; set; } = false;
    public string? Notes { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public GolfCourse Course { get; set; } = null!;
    public ICollection<TeeTimeBooking> Bookings { get; set; } = new List<TeeTimeBooking>();
}
