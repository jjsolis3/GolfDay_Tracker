using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Notifications;

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

    public List<Notification> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
        UnreadCount = Notifications.Count(n => !n.IsRead);

        // Mark all as read
        foreach (var n in Notifications.Where(n => !n.IsRead))
            n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var notif = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif != null)
        {
            _db.Notifications.Remove(notif);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAllAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        var all = await _db.Notifications.Where(n => n.UserId == userId).ToListAsync();
        foreach (var n in all) _db.Notifications.Remove(n);
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "All notifications cleared.";
        return RedirectToPage();
    }
}
