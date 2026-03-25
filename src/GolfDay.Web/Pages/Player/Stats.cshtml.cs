using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Player;

[Authorize]
public class StatsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public PlayerSeasonStats? Stats { get; set; }
    public int SelectedYear { get; set; }
    public int SelectedClubId { get; set; }
    public List<Club> Clubs { get; set; } = new();
    public List<TournamentEntry> TournamentEntries { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? year, int? clubId)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        SelectedYear = year ?? DateTime.UtcNow.Year;

        // Get user's clubs
        Clubs = await _db.ClubMemberships
            .Where(m => m.UserId == user.Id && m.IsActive)
            .Select(m => m.Club)
            .ToListAsync();

        SelectedClubId = clubId ?? Clubs.FirstOrDefault()?.Id ?? 0;

        if (SelectedClubId > 0)
        {
            Stats = await _db.PlayerSeasonStats
                .FirstOrDefaultAsync(s => s.UserId == user.Id
                                       && s.ClubId == SelectedClubId
                                       && s.Year == SelectedYear);

            TournamentEntries = await _db.TournamentEntries
                .Include(e => e.Tournament)
                .Where(e => e.UserId == user.Id
                         && e.Tournament.ClubId == SelectedClubId
                         && e.Tournament.StartDate.Year == SelectedYear)
                .OrderByDescending(e => e.Tournament.StartDate)
                .ToListAsync();
        }

        return Page();
    }
}
