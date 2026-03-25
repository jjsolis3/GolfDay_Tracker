using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Clubs;

[Authorize(Policy = "AdminPolicy")]
public class AnnouncementsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notifications;

    public AnnouncementsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager, INotificationService notifications)
    {
        _db = db; _userManager = userManager; _notifications = notifications;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public GolfDay.Domain.Entities.Club? Club { get; set; }
    public List<ClubAnnouncement> Announcements { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();
        Announcements = await _db.ClubAnnouncements
            .Include(a => a.CreatedBy)
            .Where(a => a.ClubId == Id)
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(string title, string body, int target, bool isPinned, bool isPublished, string? expiresAt)
    {
        var userId = _userManager.GetUserId(User)!;
        var ann = new ClubAnnouncement
        {
            ClubId = Id, Title = title.Trim(), Body = body.Trim(),
            CreatedByUserId = userId,
            Target = (AnnouncementTarget)target,
            IsPinned = isPinned, IsPublished = isPublished,
            PublishedAt = isPublished ? DateTime.UtcNow : null,
            ExpiresAt = DateOnly.TryParse(expiresAt, out var exp) ? exp.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) : null
        };
        _db.ClubAnnouncements.Add(ann);
        await _db.SaveChangesAsync();

        if (isPublished)
        {
            var members = await _db.ClubMemberships
                .Where(m => m.ClubId == Id && m.IsActive)
                .Select(m => m.UserId).ToListAsync();
            await _notifications.SendToManyAsync(members, NotificationType.General, title, body.Length > 150 ? body[..150] + "…" : body, $"/Club/Announcements?clubId={Id}");
        }

        TempData["SuccessMessage"] = "Announcement created.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostTogglePinAsync(int announcementId)
    {
        var ann = await _db.ClubAnnouncements.FirstOrDefaultAsync(a => a.Id == announcementId && a.ClubId == Id);
        if (ann is null) return NotFound();
        ann.IsPinned = !ann.IsPinned;
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = ann.IsPinned ? "Announcement pinned." : "Announcement unpinned.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int announcementId)
    {
        var ann = await _db.ClubAnnouncements.FirstOrDefaultAsync(a => a.Id == announcementId && a.ClubId == Id);
        if (ann is not null) { _db.ClubAnnouncements.Remove(ann); await _db.SaveChangesAsync(); }
        TempData["SuccessMessage"] = "Announcement deleted.";
        return RedirectToPage(new { id = Id });
    }
}
