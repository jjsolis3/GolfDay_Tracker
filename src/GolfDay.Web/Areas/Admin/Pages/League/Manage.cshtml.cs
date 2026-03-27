using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.League;

[Authorize(Policy = "AdminPolicy")]
public class ManageModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public LeagueSeason? Season { get; set; }
    public List<LeagueEntry> Entries { get; set; } = new();
    public List<LeagueMatch> Matches { get; set; } = new();
    public List<LeagueStanding> Standings { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Season = await _db.LeagueSeasons
            .Include(s => s.Club)
            .FirstOrDefaultAsync(s => s.Id == Id);

        if (Season is null) return NotFound();

        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        Entries = await _db.LeagueEntries
            .Where(e => e.LeagueSeasonId == Id)
            .Include(e => e.User)
            .OrderBy(e => e.User.LastName)
            .ThenBy(e => e.User.FirstName)
            .ToListAsync();

        Matches = await _db.LeagueMatches
            .Where(m => m.LeagueSeasonId == Id)
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .OrderBy(m => m.RoundNumber)
            .ThenBy(m => m.ScheduledDeadline)
            .ToListAsync();

        Standings = await _db.LeagueStandings
            .Where(s => s.LeagueSeasonId == Id)
            .Include(s => s.User)
            .OrderBy(s => s.Rank)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddPlayerAsync(
        string playerEmail, double? handicapAtEntry, int? seedNumber)
    {
        var season = await _db.LeagueSeasons.FirstOrDefaultAsync(s => s.Id == Id);
        if (season is null) return NotFound();

        var user = await _userManager.FindByEmailAsync(playerEmail);
        if (user is null)
        {
            TempData["ErrorMessage"] = $"No user found with email '{playerEmail}'.";
            return RedirectToPage(new { id = Id });
        }

        var existing = await _db.LeagueEntries
            .FirstOrDefaultAsync(e => e.LeagueSeasonId == Id && e.UserId == user.Id);

        if (existing is not null)
        {
            TempData["ErrorMessage"] = "This player is already registered in the league.";
            return RedirectToPage(new { id = Id });
        }

        var entry = new LeagueEntry
        {
            LeagueSeasonId = Id,
            UserId = user.Id,
            HandicapAtEntry = handicapAtEntry,
            SeedNumber = seedNumber,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.LeagueEntries.Add(entry);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{user.FullName} has been added to the league.";
        return RedirectToPage(new { id = Id, tab = "players" });
    }

    public async Task<IActionResult> OnPostRemovePlayerAsync(int entryId)
    {
        var entry = await _db.LeagueEntries.FirstOrDefaultAsync(e => e.Id == entryId);
        if (entry is null || entry.LeagueSeasonId != Id) return NotFound();

        _db.LeagueEntries.Remove(entry);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Player removed from the league.";
        return RedirectToPage(new { id = Id, tab = "players" });
    }

    public async Task<IActionResult> OnPostGenerateScheduleAsync()
    {
        var season = await _db.LeagueSeasons.FirstOrDefaultAsync(s => s.Id == Id);
        if (season is null) return NotFound();

        var entries = await _db.LeagueEntries
            .Where(e => e.LeagueSeasonId == Id && e.IsActive)
            .ToListAsync();

        if (entries.Count < 2)
        {
            TempData["ErrorMessage"] = "At least 2 players are required to generate a schedule.";
            return RedirectToPage(new { id = Id, tab = "matches" });
        }

        // Remove existing unplayed matches
        var existingMatches = await _db.LeagueMatches
            .Where(m => m.LeagueSeasonId == Id && m.Status == MatchStatus.Scheduled)
            .ToListAsync();
        foreach (var m in existingMatches)
            _db.LeagueMatches.Remove(m);

        // Generate round-robin schedule
        var playerIds = entries.Select(e => e.UserId).ToList();
        // Add a bye if odd number
        if (playerIds.Count % 2 != 0)
            playerIds.Add("BYE");

        int n = playerIds.Count;
        int numRounds = n - 1;
        int halfSize = n / 2;

        var matchDeadlineDays = season.MatchDeadlineDays > 0 ? season.MatchDeadlineDays : 14;
        var baseDate = season.StartDate;

        var newMatches = new List<LeagueMatch>();

        for (int round = 0; round < numRounds; round++)
        {
            var deadline = baseDate.AddDays(matchDeadlineDays * (round + 1));

            for (int i = 0; i < halfSize; i++)
            {
                var p1Id = playerIds[i];
                var p2Id = playerIds[n - 1 - i];

                // Skip bye slots
                if (p1Id == "BYE" || p2Id == "BYE")
                    continue;

                newMatches.Add(new LeagueMatch
                {
                    LeagueSeasonId = Id,
                    Player1Id = p1Id,
                    Player2Id = p2Id,
                    RoundNumber = round + 1,
                    ScheduledDeadline = deadline,
                    Status = MatchStatus.Scheduled,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Rotate: keep first fixed, rotate the rest
            var last = playerIds[n - 1];
            for (int i = n - 1; i > 1; i--)
                playerIds[i] = playerIds[i - 1];
            playerIds[1] = last;
        }

        foreach (var match in newMatches)
            _db.LeagueMatches.Add(match);

        season.Status = LeagueStatus.Active;
        season.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Schedule generated with {newMatches.Count} matches across {numRounds} rounds.";
        return RedirectToPage(new { id = Id, tab = "matches" });
    }

    public async Task<IActionResult> OnPostRecordResultAsync(
        int matchId, string outcome, int? player1NetScore,
        int? player2NetScore, DateTime? playedDate, string? resultNotes)
    {
        var match = await _db.LeagueMatches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match is null || match.LeagueSeasonId != Id) return NotFound();

        match.Player1NetScore = player1NetScore;
        match.Player2NetScore = player2NetScore;
        // HTML type="date" produces Kind=Unspecified; tag it as UTC explicitly.
        match.PlayedDate = playedDate.HasValue
            ? DateTime.SpecifyKind(playedDate.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;
        match.Notes = resultNotes;
        match.Status = MatchStatus.Completed;
        match.UpdatedAt = DateTime.UtcNow;

        match.IsDraw = false;
        match.WinnerId = null;

        switch (outcome)
        {
            case "player1":
                match.WinnerId = match.Player1Id;
                break;
            case "player2":
                match.WinnerId = match.Player2Id;
                break;
            case "draw":
                match.IsDraw = true;
                break;
        }

        await _db.SaveChangesAsync();

        // Update standings
        await RecalculateStandingsInternalAsync();

        TempData["SuccessMessage"] = "Match result recorded successfully.";
        return RedirectToPage(new { id = Id, tab = "matches" });
    }

    public async Task<IActionResult> OnPostRecalculateStandingsAsync()
    {
        var season = await _db.LeagueSeasons.FirstOrDefaultAsync(s => s.Id == Id);
        if (season is null) return NotFound();

        await RecalculateStandingsInternalAsync();

        TempData["SuccessMessage"] = "Standings recalculated successfully.";
        return RedirectToPage(new { id = Id, tab = "standings" });
    }

    private async Task RecalculateStandingsInternalAsync()
    {
        var season = await _db.LeagueSeasons.FirstOrDefaultAsync(s => s.Id == Id);
        if (season is null) return;

        var completedMatches = await _db.LeagueMatches
            .Where(m => m.LeagueSeasonId == Id && m.Status == MatchStatus.Completed)
            .ToListAsync();

        var entries = await _db.LeagueEntries
            .Where(e => e.LeagueSeasonId == Id && e.IsActive)
            .ToListAsync();

        // Build stats per player
        var stats = new Dictionary<string, (int Played, int Wins, int Draws, int Losses, int Points,
            int TotalStrokes, int? BestScore)>();

        foreach (var entry in entries)
            stats[entry.UserId] = (0, 0, 0, 0, 0, 0, null);

        foreach (var match in completedMatches)
        {
            void UpdatePlayer(string playerId, bool isWinner, bool isDraw, int? netScore)
            {
                if (!stats.ContainsKey(playerId)) return;
                var (played, wins, draws, losses, points, totalStrokes, bestScore) = stats[playerId];
                played++;
                if (isDraw)
                {
                    draws++;
                    points += season.PointsForDraw;
                }
                else if (isWinner)
                {
                    wins++;
                    points += season.PointsForWin;
                }
                else
                {
                    losses++;
                    points += season.PointsForLoss;
                }
                if (netScore.HasValue)
                {
                    totalStrokes += netScore.Value;
                    bestScore = bestScore.HasValue ? Math.Min(bestScore.Value, netScore.Value) : netScore.Value;
                }
                stats[playerId] = (played, wins, draws, losses, points, totalStrokes, bestScore);
            }

            UpdatePlayer(match.Player1Id, match.WinnerId == match.Player1Id, match.IsDraw, match.Player1NetScore);
            UpdatePlayer(match.Player2Id, match.WinnerId == match.Player2Id, match.IsDraw, match.Player2NetScore);
        }

        // Remove old standings
        var oldStandings = await _db.LeagueStandings
            .Where(s => s.LeagueSeasonId == Id)
            .ToListAsync();
        foreach (var s in oldStandings)
            _db.LeagueStandings.Remove(s);

        // Create new standings, sorted by points desc then wins desc
        var sorted = stats
            .Where(kv => kv.Value.Played > 0)
            .OrderByDescending(kv => kv.Value.Points)
            .ThenByDescending(kv => kv.Value.Wins)
            .ThenBy(kv => kv.Value.TotalStrokes > 0 && kv.Value.Played > 0
                ? (double)kv.Value.TotalStrokes / kv.Value.Played
                : double.MaxValue)
            .ToList();

        int rank = 1;
        foreach (var (userId, s) in sorted)
        {
            double? avg = s.Played > 0 && s.TotalStrokes > 0
                ? (double)s.TotalStrokes / s.Played
                : null;

            _db.LeagueStandings.Add(new LeagueStanding
            {
                LeagueSeasonId = Id,
                UserId = userId,
                Rank = rank++,
                Played = s.Played,
                Wins = s.Wins,
                Draws = s.Draws,
                Losses = s.Losses,
                Points = s.Points,
                TotalStrokes = s.TotalStrokes > 0 ? s.TotalStrokes : null,
                AverageScore = avg,
                BestScore = s.BestScore,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
    }
}
