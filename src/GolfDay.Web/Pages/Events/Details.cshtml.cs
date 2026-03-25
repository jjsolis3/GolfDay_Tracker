using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Events;

public class LeaderboardEntry
{
    public int RoundId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int HolesCompleted { get; set; }
    public int? GrossScore { get; set; }
    public int? NetScore { get; set; }
    public int? Birdies { get; set; }
    public int ScoreToPar { get; set; }
    public bool IsInProgress { get; set; }
}

public class EventDetailsModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public EventDetailsModel(IApplicationDbContext db)
    {
        _db = db;
    }

    public GolfEvent? Event { get; set; }
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _db.GolfEvents
            .Include(e => e.Club)
            .Include(e => e.Course)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.User)
            .Include(e => e.Rounds)
                .ThenInclude(r => r.HoleScores)
            .Include(e => e.Achievements)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(e => e.Id == id && e.IsPublic);

        if (Event == null)
            return Page();

        // Build leaderboard for InProgress or Completed events
        if (Event.Status == EventStatus.InProgress || Event.Status == EventStatus.Completed)
        {
            var courseParTotal = Event.Course?.ParTotal ?? 72;

            Leaderboard = Event.Rounds
                .Where(r => r.Status != RoundStatus.Disqualified && r.Status != RoundStatus.Withdrawn)
                .Select(r => new LeaderboardEntry
                {
                    RoundId = r.Id,
                    PlayerName = r.User.FullName,
                    HolesCompleted = r.HoleScores.Count,
                    GrossScore = r.GrossScore,
                    NetScore = r.NetScore,
                    Birdies = r.Birdies,
                    ScoreToPar = (r.GrossScore ?? courseParTotal) - courseParTotal,
                    IsInProgress = r.Status == RoundStatus.InProgress
                })
                .OrderBy(e => e.ScoreToPar)
                .ThenByDescending(e => e.HolesCompleted)
                .ToList();
        }

        return Page();
    }
}
