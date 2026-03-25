using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolfDay.Web.Pages.Club;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public DashboardModel(IApplicationDbContext db) => _db = db;

    public Domain.Entities.Club? Club { get; set; }
    public ClubRole UserRole { get; set; } = ClubRole.Member;
    public int MemberCount { get; set; }
    public int EventsThisYear { get; set; }
    public int TournamentsThisYear { get; set; }
    public int HoleInOnesThisYear { get; set; }
    public List<GolfEvent> UpcomingEvents { get; set; } = new();
    public List<LeagueSeason> ActiveLeagues { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        Club = await _db.Clubs
            .Include(c => c.Courses.Where(co => co.IsActive))
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive);

        if (Club == null) return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var membership = await _db.ClubMemberships
            .FirstOrDefaultAsync(m => m.ClubId == Club.Id && m.UserId == userId && m.IsActive);

        UserRole = membership?.Role ?? ClubRole.Member;

        MemberCount = await _db.ClubMemberships.CountAsync(m => m.ClubId == Club.Id && m.IsActive);

        var thisYear = DateTime.UtcNow.Year;

        EventsThisYear = await _db.GolfEvents
            .CountAsync(e => e.ClubId == Club.Id && e.EventDate.Year == thisYear && e.Status != EventStatus.Cancelled);

        TournamentsThisYear = await _db.Tournaments
            .CountAsync(t => t.ClubId == Club.Id && t.StartDate.Year == thisYear && t.Status != TournamentStatus.Cancelled);

        HoleInOnesThisYear = await _db.HoleScores
            .Where(h => h.IsHoleInOne && h.Round.Event.ClubId == Club.Id && h.CreatedAt.Year == thisYear)
            .CountAsync();

        UpcomingEvents = await _db.GolfEvents
            .Include(e => e.Course)
            .Where(e => e.ClubId == Club.Id
                     && (e.Status == EventStatus.Scheduled || e.Status == EventStatus.InProgress)
                     && e.EventDate >= DateTime.UtcNow.AddDays(-1))
            .OrderBy(e => e.EventDate)
            .Take(8)
            .ToListAsync();

        ActiveLeagues = await _db.LeagueSeasons
            .Include(l => l.Entries)
            .Where(l => l.ClubId == Club.Id && l.Status == LeagueStatus.Active)
            .ToListAsync();

        return Page();
    }
}
