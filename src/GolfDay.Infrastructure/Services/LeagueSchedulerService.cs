using GolfDay.Application.Common.Interfaces;
using GolfDay.Application.Services;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Infrastructure.Services;

public class LeagueSchedulerService : ILeagueSchedulerService
{
    private readonly IApplicationDbContext _db;

    public LeagueSchedulerService(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<LeagueMatch>> GenerateRoundRobinScheduleAsync(int leagueSeasonId, CancellationToken ct = default)
    {
        var season = await _db.LeagueSeasons
            .Include(l => l.Entries)
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Id == leagueSeasonId, ct);

        if (season == null) throw new InvalidOperationException("League season not found.");
        if (season.Matches.Any()) throw new InvalidOperationException("Schedule already generated.");

        var players = season.Entries.Where(e => e.IsActive).Select(e => e.UserId).ToList();
        if (players.Count < 2) throw new InvalidOperationException("Need at least 2 players.");

        // Round-robin using the circle method
        var matches = new List<LeagueMatch>();
        int numPlayers = players.Count;
        if (numPlayers % 2 != 0) players.Add("BYE");

        int n = players.Count;
        int numRounds = n - 1;
        int matchesPerRound = n / 2;
        var seasonDuration = (season.EndDate - season.StartDate).TotalDays;
        double daysPerRound = seasonDuration / numRounds;

        int roundNum = 1;
        for (int round = 0; round < numRounds; round++)
        {
            var deadline = season.StartDate.AddDays(daysPerRound * (round + 1));

            for (int match = 0; match < matchesPerRound; match++)
            {
                int home = (round + match) % (n - 1);
                int away = (n - 1 - match + round) % (n - 1);
                if (match == 0) away = n - 1;

                var p1 = players[home];
                var p2 = players[away];

                if (p1 == "BYE" || p2 == "BYE") continue;

                matches.Add(new LeagueMatch
                {
                    LeagueSeasonId = leagueSeasonId,
                    Player1Id = p1,
                    Player2Id = p2,
                    ScheduledDeadline = deadline,
                    Status = MatchStatus.Scheduled,
                    RoundNumber = roundNum
                });
            }
            roundNum++;
        }

        _db.LeagueMatches.AddRange(matches);
        await _db.SaveChangesAsync(ct);
        return matches;
    }

    public async Task<IEnumerable<LeagueMatch>> GenerateSingleEliminationBracketAsync(int leagueSeasonId, CancellationToken ct = default)
    {
        var season = await _db.LeagueSeasons
            .Include(l => l.Entries)
            .FirstOrDefaultAsync(l => l.Id == leagueSeasonId, ct);

        if (season == null) throw new InvalidOperationException("League season not found.");

        var players = season.Entries.Where(e => e.IsActive)
                           .OrderBy(e => e.SeedNumber ?? 999)
                           .Select(e => e.UserId).ToList();

        // Pad to next power of 2
        int bracketSize = 1;
        while (bracketSize < players.Count) bracketSize *= 2;

        var matches = new List<LeagueMatch>();
        var seasonDuration = (season.EndDate - season.StartDate).TotalDays;
        int numRounds = (int)Math.Log2(bracketSize);
        double daysPerRound = seasonDuration / numRounds;

        // First round only — subsequent rounds created as matches complete
        for (int i = 0; i < bracketSize / 2; i++)
        {
            var p1 = i < players.Count ? players[i] : null;
            var p2 = (bracketSize - 1 - i) < players.Count ? players[bracketSize - 1 - i] : null;

            if (p1 == null) continue;
            if (p2 == null) continue; // BYE - auto-advance p1

            matches.Add(new LeagueMatch
            {
                LeagueSeasonId = leagueSeasonId,
                Player1Id = p1,
                Player2Id = p2,
                ScheduledDeadline = season.StartDate.AddDays(daysPerRound),
                Status = MatchStatus.Scheduled,
                RoundNumber = 1
            });
        }

        _db.LeagueMatches.AddRange(matches);
        await _db.SaveChangesAsync(ct);
        return matches;
    }

    public async Task MarkOverdueMatchesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var overdueMatches = await _db.LeagueMatches
            .Where(m => m.Status == MatchStatus.Scheduled && m.ScheduledDeadline < now)
            .ToListAsync(ct);

        foreach (var match in overdueMatches)
            match.Status = MatchStatus.Overdue;

        if (overdueMatches.Any())
            await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<LeagueMatch>> GetUpcomingMatchesForPlayerAsync(string userId, CancellationToken ct = default)
    {
        return await _db.LeagueMatches
            .Include(m => m.LeagueSeason)
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Course)
            .Where(m => (m.Player1Id == userId || m.Player2Id == userId)
                     && (m.Status == MatchStatus.Scheduled || m.Status == MatchStatus.Overdue)
                     && m.LeagueSeason.Status == LeagueStatus.Active)
            .OrderBy(m => m.ScheduledDeadline)
            .ToListAsync(ct);
    }
}
