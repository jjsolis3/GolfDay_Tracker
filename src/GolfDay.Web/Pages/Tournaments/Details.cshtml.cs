using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
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
    public Dictionary<(string UserId, int RoundNumber), int> RoundScores { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tournament = await _db.Tournaments
            .Include(t => t.Club)
            .Include(t => t.Course)
            .Include(t => t.Entries).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (Tournament == null) return Page();

        RankedEntries = Tournament.Entries
            .Where(e => e.Status != Domain.Enums.EntryStatus.Withdrawn && e.TotalGrossScore.HasValue)
            .OrderBy(e => e.TotalGrossScore)
            .ToList();

        // Load round-by-round scores
        var rounds = await _db.Rounds
            .Where(r => r.Event.TournamentId == id && r.GrossScore.HasValue)
            .Select(r => new { r.UserId, r.RoundNumber, r.GrossScore })
            .ToListAsync();

        foreach (var r in rounds)
            RoundScores[(r.UserId, r.RoundNumber)] = r.GrossScore ?? 0;

        return Page();
    }
}
