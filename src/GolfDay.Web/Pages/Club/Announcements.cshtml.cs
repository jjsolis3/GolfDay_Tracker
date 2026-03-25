using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Club;

public class AnnouncementsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AnnouncementsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    { _db = db; _userManager = userManager; }

    [BindProperty(SupportsGet = true)] public int ClubId { get; set; }

    public GolfDay.Domain.Entities.Club? Club { get; set; }
    public List<ClubAnnouncement> Announcements { get; set; } = new();
    public bool IsManager { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId);
        if (Club is null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (userId != null)
        {
            IsManager = await _db.ClubMemberships.AnyAsync(m =>
                m.ClubId == ClubId && m.UserId == userId && m.IsActive &&
                (m.Role == ClubRole.Owner || m.Role == ClubRole.Manager));
        }

        var now = DateTime.UtcNow;
        var query = _db.ClubAnnouncements
            .Include(a => a.CreatedBy)
            .Where(a => a.ClubId == ClubId && a.IsPublished &&
                        (a.ExpiresAt == null || a.ExpiresAt > now));

        if (!IsManager)
            query = query.Where(a => a.Target != AnnouncementTarget.ManagersOnly);

        Announcements = await query
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .ToListAsync();

        return Page();
    }
}
