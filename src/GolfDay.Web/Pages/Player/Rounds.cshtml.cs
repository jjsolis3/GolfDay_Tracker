using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Player;

[Authorize]
public class RoundsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoundsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<Round> Rounds { get; set; } = new();
    public int SelectedYear { get; set; }
    public int SelectedClubId { get; set; }
    public List<Club> Clubs { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? year, int? clubId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        SelectedYear = year ?? DateTime.UtcNow.Year;

        Clubs = await _db.ClubMemberships
            .Where(m => m.UserId == user.Id && m.IsActive)
            .Select(m => m.Club)
            .ToListAsync();

        SelectedClubId = clubId ?? 0;

        var query = _db.Rounds
            .Include(r => r.Event).ThenInclude(e => e.Club)
            .Include(r => r.Course)
            .Where(r => r.UserId == user.Id
                     && r.Status == RoundStatus.Completed
                     && r.CompletedAt.HasValue
                     && r.CompletedAt.Value.Year == SelectedYear);

        if (SelectedClubId > 0)
            query = query.Where(r => r.Event.ClubId == SelectedClubId);

        Rounds = await query
            .OrderByDescending(r => r.CompletedAt)
            .ToListAsync();

        return Page();
    }
}
