using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Tournaments;

public class TournamentDetailsModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public TournamentDetailsModel(IApplicationDbContext db) => _db = db;

    public Tournament? Tournament { get; set; }
    public List<TournamentEntry> RankedEntries { get; set; } = new();

    /// <summary>Key: (UserId, RoundNumber) → Gross Score for that round.</summary>
    public Dictionary<(string UserId, int RoundNumber), int> RoundScores { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tournament = await _db.Tournaments
            .Include(t => t.Club)
            .Include(t => t.Course)
            .Include(t => t.Entries)
                .ThenInclude(e => e.User)
            .Include(t => t.Rounds)
                .ThenInclude(ev => ev.Course)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (Tournament == null)
            return Page();

        // Build ranked leaderboard — players who have at least started
        RankedEntries = Tournament.Entries
            .Where(e => e.Status != EntryStatus.Withdrawn
                     && e.Status != EntryStatus.Disqualified
                     && e.TotalGrossScore.HasValue)
            .OrderBy(e => e.TotalGrossScore)
            .ThenBy(e => e.TotalNetScore)
            .ToList();

        // Assign final position if not already set
        for (int i = 0; i < RankedEntries.Count; i++)
        {
            if (!RankedEntries[i].FinalPosition.HasValue)
                RankedEntries[i].FinalPosition = i + 1;
        }

        // Load round-by-round scores keyed by (userId, roundNumber)
        var rounds = await _db.Rounds
            .Where(r => r.Event.TournamentId == id && r.GrossScore.HasValue)
            .Select(r => new { r.UserId, r.RoundNumber, r.GrossScore })
            .ToListAsync();

        foreach (var r in rounds)
        {
            RoundScores[(r.UserId, r.RoundNumber)] = r.GrossScore ?? 0;
        }

        return Page();
    }
}
