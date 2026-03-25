using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Events;

[Authorize]
public class WaitlistModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notifications;

    public WaitlistModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager, INotificationService notifications)
    { _db = db; _userManager = userManager; _notifications = notifications; }

    [BindProperty(SupportsGet = true)] public int EventId { get; set; }

    public GolfEvent? Event { get; set; }
    public List<EventWaitlistEntry> WaitlistEntries { get; set; } = new();
    public EventWaitlistEntry? MyEntry { get; set; }
    public bool IsParticipant { get; set; }
    public int ParticipantCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Event = await _db.GolfEvents.Include(e => e.Club).FirstOrDefaultAsync(e => e.Id == EventId);
        if (Event is null) return NotFound();

        ParticipantCount = await _db.EventParticipants.CountAsync(p => p.EventId == EventId);
        IsParticipant = await _db.EventParticipants.AnyAsync(p => p.EventId == EventId && p.UserId == userId);

        WaitlistEntries = await _db.EventWaitlistEntries
            .Include(w => w.User)
            .Where(w => w.EventId == EventId && w.Status == WaitlistStatus.Waiting)
            .OrderBy(w => w.Position)
            .ToListAsync();

        MyEntry = WaitlistEntries.FirstOrDefault(w => w.UserId == userId)
                  ?? await _db.EventWaitlistEntries.FirstOrDefaultAsync(w => w.EventId == EventId && w.UserId == userId);

        return Page();
    }

    public async Task<IActionResult> OnPostJoinAsync()
    {
        var userId = _userManager.GetUserId(User)!;

        var existing = await _db.EventWaitlistEntries
            .FirstOrDefaultAsync(w => w.EventId == EventId && w.UserId == userId);
        if (existing != null)
        {
            TempData["ErrorMessage"] = "You are already on the waitlist.";
            return RedirectToPage(new { eventId = EventId });
        }

        var maxPos = await _db.EventWaitlistEntries
            .Where(w => w.EventId == EventId && w.Status == WaitlistStatus.Waiting)
            .MaxAsync(w => (int?)w.Position) ?? 0;

        _db.EventWaitlistEntries.Add(new EventWaitlistEntry
        {
            EventId = EventId, UserId = userId,
            Position = maxPos + 1,
            Status = WaitlistStatus.Waiting
        });
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"You've been added to the waitlist at position {maxPos + 1}.";
        return RedirectToPage(new { eventId = EventId });
    }

    public async Task<IActionResult> OnPostLeaveAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        var entry = await _db.EventWaitlistEntries
            .FirstOrDefaultAsync(w => w.EventId == EventId && w.UserId == userId);
        if (entry != null)
        {
            entry.Status = WaitlistStatus.Cancelled;
            await _db.SaveChangesAsync();
            // Renumber remaining entries
            var remaining = await _db.EventWaitlistEntries
                .Where(w => w.EventId == EventId && w.Status == WaitlistStatus.Waiting)
                .OrderBy(w => w.Position).ToListAsync();
            for (int i = 0; i < remaining.Count; i++) remaining[i].Position = i + 1;
            await _db.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "You've been removed from the waitlist.";
        return RedirectToPage(new { eventId = EventId });
    }
}
