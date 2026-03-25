using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolfDay.Web.Pages.League;

public class LeagueDetailsModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public LeagueDetailsModel(IApplicationDbContext db) => _db = db;

    public LeagueSeason? League { get; set; }
    public List<LeagueMatch> UserMatches { get; set; } = new();
    public string? CurrentUserId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        League = await _db.LeagueSeasons
            .Include(l => l.Club)
            .Include(l => l.Entries).ThenInclude(e => e.User)
            .Include(l => l.Matches).ThenInclude(m => m.Player1)
            .Include(l => l.Matches).ThenInclude(m => m.Player2)
            .Include(l => l.Matches).ThenInclude(m => m.Course)
            .Include(l => l.Standings).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (League == null) return Page();

        CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (CurrentUserId != null)
        {
            UserMatches = League.Matches
                .Where(m => (m.Player1Id == CurrentUserId || m.Player2Id == CurrentUserId)
                         && (m.Status == MatchStatus.Scheduled || m.Status == MatchStatus.Overdue))
                .OrderBy(m => m.ScheduledDeadline)
                .ToList();
        }

        return Page();
    }
}
