using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolfDay.Web.Pages.Scoring;

public class LiveModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public LiveModel(IApplicationDbContext db) => _db = db;

    public GolfEvent? Event { get; set; }
    public Round? UserRound { get; set; }
    public List<HoleScore> HoleScores { get; set; } = new();
    public List<CourseHole> CourseHoles { get; set; } = new();
    public List<LeaderboardEntry> Leaderboard { get; set; } = new();

    public class LeaderboardEntry
    {
        public int RoundId { get; set; }
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string? HandicapStr { get; set; }
        public int? GrossScore { get; set; }
        public int? NetScore { get; set; }
        public int HolesCompleted { get; set; }
        public RoundStatus Status { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _db.GolfEvents
            .Include(e => e.Course)
            .Include(e => e.Club)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (Event == null)
            return Page();

        // Load course holes for scorecard par display
        if (Event.CourseId.HasValue)
        {
            CourseHoles = await _db.CourseHoles
                .Where(h => h.CourseId == Event.CourseId)
                .OrderBy(h => h.HoleNumber)
                .ToListAsync();
        }

        // Load the current user's round and hole scores
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            UserRound = await _db.Rounds
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (UserRound != null)
            {
                HoleScores = await _db.HoleScores
                    .Where(h => h.RoundId == UserRound.Id)
                    .OrderBy(h => h.HoleNumber)
                    .ToListAsync();
            }
        }

        // Build leaderboard sorted by gross score ascending
        var rounds = await _db.Rounds
            .Include(r => r.User)
            .Include(r => r.HoleScores)
            .Where(r => r.EventId == eventId
                     && r.Status != RoundStatus.Withdrawn
                     && r.Status != RoundStatus.Disqualified)
            .ToListAsync();

        Leaderboard = rounds
            .Select(r => new LeaderboardEntry
            {
                RoundId        = r.Id,
                PlayerId       = r.UserId,
                PlayerName     = r.User.FullName,
                HandicapStr    = r.User.HandicapIndex.HasValue ? r.User.HandicapIndex.Value.ToString("F1") : null,
                GrossScore     = r.GrossScore,
                NetScore       = r.NetScore,
                HolesCompleted = r.HoleScores.Count,
                Status         = r.Status
            })
            .OrderBy(e => e.GrossScore ?? int.MaxValue)
            .ThenByDescending(e => e.HolesCompleted)
            .ToList();

        return Page();
    }
}
