using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClubEntity = GolfDay.Domain.Entities.Club;

namespace GolfDay.Web.Pages.TeeTime;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int ClubId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? Date { get; set; }

    public List<ClubEntity> MyClubs { get; set; } = new();
    public ClubEntity? SelectedClub { get; set; }
    public List<TeeTimeSlot> AvailableSlots { get; set; } = new();
    public List<TeeTimeBooking> MyBookings { get; set; } = new();
    public DateOnly SelectedDate { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        SelectedDate = Date.HasValue
            ? DateOnly.FromDateTime(Date.Value)
            : DateOnly.FromDateTime(DateTime.UtcNow);

        MyClubs = await _db.ClubMemberships
            .Where(m => m.UserId == userId && m.IsActive)
            .Select(m => m.Club)
            .OrderBy(c => c.Name)
            .ToListAsync();

        if (ClubId > 0)
        {
            SelectedClub = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId);

            AvailableSlots = await _db.TeeTimeSlots
                .Include(s => s.Course)
                .Include(s => s.Bookings)
                .Where(s => s.ClubId == ClubId && s.SlotDate == SelectedDate && s.IsActive && !s.IsBlocked)
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        MyBookings = await _db.TeeTimeBookings
            .Include(b => b.TeeTimeSlot).ThenInclude(s => s.Course)
            .Where(b => b.UserId == userId
                && b.Status == TeeTimeBookingStatus.Confirmed
                && b.TeeTimeSlot.SlotDate >= today)
            .OrderBy(b => b.TeeTimeSlot.SlotDate)
            .ThenBy(b => b.TeeTimeSlot.StartTime)
            .Take(5)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostBookAsync(int slotId, int numberOfPlayers = 1)
    {
        var userId = _userManager.GetUserId(User)!;

        var slot = await _db.TeeTimeSlots
            .Include(s => s.Bookings.Where(b => b.Status == TeeTimeBookingStatus.Confirmed))
            .FirstOrDefaultAsync(s => s.Id == slotId && s.IsActive && !s.IsBlocked);

        if (slot == null)
        {
            TempData["ErrorMessage"] = "This tee time is no longer available.";
            return RedirectToPage(new { clubId = ClubId, date = Date });
        }

        var existingBooking = slot.Bookings.FirstOrDefault(b => b.UserId == userId);
        if (existingBooking != null)
        {
            TempData["ErrorMessage"] = "You already have a booking for this tee time.";
            return RedirectToPage(new { clubId = ClubId, date = Date });
        }

        var bookedCount = slot.Bookings.Sum(b => b.NumberOfPlayers);
        if (bookedCount + numberOfPlayers > slot.MaxPlayers)
        {
            TempData["ErrorMessage"] = "Not enough spots available for your group size.";
            return RedirectToPage(new { clubId = ClubId, date = Date });
        }

        _db.TeeTimeBookings.Add(new TeeTimeBooking
        {
            TeeTimeSlotId = slotId,
            UserId = userId,
            NumberOfPlayers = numberOfPlayers,
            Status = TeeTimeBookingStatus.Confirmed
        });
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Tee time booked for {slot.StartTime:h:mm tt}!";
        return RedirectToPage(new { clubId = ClubId, date = Date });
    }

    public async Task<IActionResult> OnPostCancelAsync(int bookingId)
    {
        var userId = _userManager.GetUserId(User)!;

        var booking = await _db.TeeTimeBookings
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null) return NotFound();

        booking.Status = TeeTimeBookingStatus.Cancelled;
        booking.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tee time cancelled.";
        return RedirectToPage(new { clubId = ClubId, date = Date });
    }
}
