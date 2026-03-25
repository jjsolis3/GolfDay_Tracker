using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClubEntity = GolfDay.Domain.Entities.Club;

namespace GolfDay.Web.Areas.Admin.Pages.TeeTime;

[Authorize(Policy = "AdminPolicy")]
public class ManageModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public ManageModel(IApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; } // ClubId

    [BindProperty(SupportsGet = true)]
    public string? Date { get; set; }

    public ClubEntity? Club { get; set; }
    public List<GolfCourse> Courses { get; set; } = new();
    public List<TeeTimeSlot> Slots { get; set; } = new();
    public DateOnly SelectedDate { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();

        SelectedDate = DateOnly.TryParse(Date, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow);
        Courses = await _db.GolfCourses.Where(c => c.ClubId == Id && c.IsActive).OrderBy(c => c.Name).ToListAsync();

        Slots = await _db.TeeTimeSlots
            .Include(s => s.Course)
            .Include(s => s.Bookings).ThenInclude(b => b.User)
            .Where(s => s.ClubId == Id && s.SlotDate == SelectedDate)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAddSlotAsync(int courseId, string startTime, int maxPlayers, string? notes)
    {
        if (!TimeOnly.TryParse(startTime, out var time))
        {
            TempData["ErrorMessage"] = "Invalid start time.";
            return RedirectToPage(new { id = Id, date = Date });
        }

        var date = DateOnly.TryParse(Date, out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow);

        _db.TeeTimeSlots.Add(new TeeTimeSlot
        {
            ClubId = Id,
            CourseId = courseId,
            SlotDate = date,
            StartTime = time,
            MaxPlayers = maxPlayers > 0 ? maxPlayers : 4,
            Notes = notes?.Trim(),
            IsActive = true
        });
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tee time slot added.";
        return RedirectToPage(new { id = Id, date = Date });
    }

    public async Task<IActionResult> OnPostBulkGenerateAsync(
        int courseId, string startDate, string endDate, string firstTeeTime,
        int intervalMinutes, int slotsPerDay, int maxPlayers)
    {
        if (!DateOnly.TryParse(startDate, out var sd) ||
            !DateOnly.TryParse(endDate, out var ed) ||
            !TimeOnly.TryParse(firstTeeTime, out var firstTime))
        {
            TempData["ErrorMessage"] = "Invalid date or time values.";
            return RedirectToPage(new { id = Id, date = Date });
        }

        if (sd > ed || slotsPerDay < 1 || intervalMinutes < 5)
        {
            TempData["ErrorMessage"] = "Invalid generation parameters.";
            return RedirectToPage(new { id = Id, date = Date });
        }

        int added = 0;
        for (var day = sd; day <= ed; day = day.AddDays(1))
        {
            for (int i = 0; i < slotsPerDay; i++)
            {
                var slotTime = firstTime.AddMinutes(i * intervalMinutes);
                var exists = await _db.TeeTimeSlots.AnyAsync(s =>
                    s.ClubId == Id && s.CourseId == courseId &&
                    s.SlotDate == day && s.StartTime == slotTime);

                if (!exists)
                {
                    _db.TeeTimeSlots.Add(new TeeTimeSlot
                    {
                        ClubId = Id,
                        CourseId = courseId,
                        SlotDate = day,
                        StartTime = slotTime,
                        MaxPlayers = maxPlayers,
                        IsActive = true
                    });
                    added++;
                }
            }
        }
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{added} tee time slot(s) generated.";
        return RedirectToPage(new { id = Id, date = startDate });
    }

    public async Task<IActionResult> OnPostDeleteSlotAsync(int slotId)
    {
        var slot = await _db.TeeTimeSlots
            .Include(s => s.Bookings)
            .FirstOrDefaultAsync(s => s.Id == slotId && s.ClubId == Id);

        if (slot is null) return NotFound();

        foreach (var booking in slot.Bookings)
            _db.TeeTimeBookings.Remove(booking);

        _db.TeeTimeSlots.Remove(slot);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tee time slot deleted.";
        return RedirectToPage(new { id = Id, date = Date });
    }

    public async Task<IActionResult> OnPostToggleBlockAsync(int slotId)
    {
        var slot = await _db.TeeTimeSlots.FirstOrDefaultAsync(s => s.Id == slotId && s.ClubId == Id);
        if (slot is null) return NotFound();

        slot.IsBlocked = !slot.IsBlocked;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = slot.IsBlocked ? "Slot blocked." : "Slot unblocked.";
        return RedirectToPage(new { id = Id, date = Date });
    }
}
