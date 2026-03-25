using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public int TotalClubs { get; set; }
    public int ActiveEvents { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveLeagues { get; set; }
    public int TotalTournaments { get; set; }
    public int TotalLeagueSeasons { get; set; }
    public int TotalCourses { get; set; }
    public int TotalRounds { get; set; }

    public List<GolfEvent> RecentEvents { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalClubs = await _db.Clubs.CountAsync(c => c.IsActive);

        ActiveEvents = await _db.GolfEvents.CountAsync(e =>
            e.Status == EventStatus.Scheduled || e.Status == EventStatus.InProgress);

        TotalMembers = await _db.ClubMemberships.CountAsync(m => m.IsActive);

        ActiveLeagues = await _db.LeagueSeasons.CountAsync(l =>
            l.Status == LeagueStatus.Active || l.Status == LeagueStatus.Registration);

        TotalTournaments = await _db.Tournaments.CountAsync();
        TotalLeagueSeasons = await _db.LeagueSeasons.CountAsync();
        TotalCourses = await _db.GolfCourses.CountAsync(c => c.IsActive);
        TotalRounds = await _db.Rounds.CountAsync();

        RecentEvents = await _db.GolfEvents
            .Include(e => e.Club)
            .Include(e => e.Participants)
            .OrderByDescending(e => e.CreatedAt)
            .Take(10)
            .ToListAsync();
    }
}
