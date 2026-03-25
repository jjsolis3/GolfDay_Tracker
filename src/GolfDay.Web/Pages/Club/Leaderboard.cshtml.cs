using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolfDay.Web.Pages.Club;

public class LeaderboardModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public LeaderboardModel(IApplicationDbContext db) => _db = db;

    public List<PlayerSeasonStats> Entries { get; set; } = new();
    public List<Club> Clubs { get; set; } = new();
    public int SelectedClubId { get; set; }
    public int SelectedYear { get; set; }

    public async Task<IActionResult> OnGetAsync(int? clubId, int? year)
    {
        SelectedYear = year ?? DateTime.UtcNow.Year;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get clubs visible to the current user (public clubs or clubs they belong to)
        if (userId != null)
        {
            Clubs = await _db.ClubMemberships
                .Where(m => m.UserId == userId && m.IsActive)
                .Select(m => m.Club)
                .ToListAsync();
        }

        if (!Clubs.Any())
        {
            Clubs = await _db.Clubs
                .Where(c => c.IsActive && c.IsPublic)
                .Take(10)
                .ToListAsync();
        }

        SelectedClubId = clubId ?? Clubs.FirstOrDefault()?.Id ?? 0;

        if (SelectedClubId > 0)
        {
            Entries = await _db.PlayerSeasonStats
                .Include(s => s.User)
                .Where(s => s.ClubId == SelectedClubId && s.Year == SelectedYear && s.RoundsPlayed > 0)
                .OrderBy(s => s.AverageGrossScore)
                .ToListAsync();
        }

        return Page();
    }
}
