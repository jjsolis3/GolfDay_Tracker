using GolfDay.Application.Common.Interfaces;
using GolfDay.Application.Services;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Infrastructure.Services;

public class StatsService : IStatsService
{
    private readonly IApplicationDbContext _db;

    public StatsService(IApplicationDbContext db) => _db = db;

    public async Task RecalculatePlayerSeasonStatsAsync(string userId, int clubId, int year, CancellationToken ct = default)
    {
        var rounds = await _db.Rounds
            .Include(r => r.HoleScores)
            .Include(r => r.Event)
            .Where(r => r.UserId == userId
                     && r.Event.ClubId == clubId
                     && r.Status == RoundStatus.Completed
                     && r.CompletedAt.HasValue
                     && r.CompletedAt.Value.Year == year)
            .ToListAsync(ct);

        var stats = await _db.PlayerSeasonStats
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ClubId == clubId && s.Year == year, ct);

        if (stats == null)
        {
            stats = new PlayerSeasonStats { UserId = userId, ClubId = clubId, Year = year };
            _db.PlayerSeasonStats.Add(stats);
        }

        stats.RoundsPlayed = rounds.Count;
        stats.LastUpdated = DateTime.UtcNow;

        if (!rounds.Any()) { await _db.SaveChangesAsync(ct); return; }

        var grossScores = rounds.Where(r => r.GrossScore.HasValue).Select(r => r.GrossScore!.Value).ToList();
        if (grossScores.Any())
        {
            stats.BestGrossScore = grossScores.Min();
            stats.WorstGrossScore = grossScores.Max();
            stats.AverageGrossScore = Math.Round(grossScores.Average(), 1);
        }

        var netScores = rounds.Where(r => r.NetScore.HasValue).Select(r => r.NetScore!.Value).ToList();
        if (netScores.Any())
            stats.AverageNetScore = Math.Round(netScores.Average(), 1);

        stats.TotalBirdiesOrBetter = rounds.Sum(r => r.Birdies ?? 0);
        stats.TotalEagles = rounds.Sum(r => r.Eagles ?? 0);
        stats.TotalHoleInOnes = rounds.Sum(r => r.HoleInOnes ?? 0);
        stats.TotalPars = rounds.Sum(r => r.Pars ?? 0);
        stats.TotalBogeys = rounds.Sum(r => r.Bogeys ?? 0);
        stats.TotalDoubleBogeys = rounds.Sum(r => r.DoubleBogeys ?? 0);
        stats.TotalTriplePlusBogeys = rounds.Sum(r => r.TriplePlusBogeys ?? 0);

        var puttsRounds = rounds.Where(r => r.TotalPutts.HasValue).ToList();
        if (puttsRounds.Any())
            stats.AveragePutts = Math.Round(puttsRounds.Average(r => r.TotalPutts!.Value), 1);

        var fairwayRounds = rounds.Where(r => r.FairwaysHit.HasValue && r.TotalFairways.HasValue && r.TotalFairways > 0).ToList();
        if (fairwayRounds.Any())
            stats.FairwayHitPercent = Math.Round(
                100.0 * fairwayRounds.Sum(r => r.FairwaysHit!.Value) / fairwayRounds.Sum(r => r.TotalFairways!.Value), 1);

        var girRounds = rounds.Where(r => r.GreensInRegulation.HasValue && r.TotalGreens.HasValue && r.TotalGreens > 0).ToList();
        if (girRounds.Any())
            stats.GIRPercent = Math.Round(
                100.0 * girRounds.Sum(r => r.GreensInRegulation!.Value) / girRounds.Sum(r => r.TotalGreens!.Value), 1);

        var longestDrives = rounds.Where(r => r.LongestDriveYards.HasValue).Select(r => r.LongestDriveYards!.Value).ToList();
        if (longestDrives.Any())
            stats.LongestDriveYards = longestDrives.Max();

        // Tournament stats
        var tournamentEntries = await _db.TournamentEntries
            .Where(te => te.UserId == userId
                      && te.Tournament.ClubId == clubId
                      && te.Tournament.StartDate.Year == year
                      && te.Status == EntryStatus.Confirmed)
            .ToListAsync(ct);

        stats.TournamentsPlayed = tournamentEntries.Count;
        var positions = tournamentEntries.Where(t => t.FinalPosition.HasValue).Select(t => t.FinalPosition!.Value).ToList();
        if (positions.Any()) stats.BestTournamentPosition = positions.Min();

        // League stats
        var leagueMatches = await _db.LeagueMatches
            .Where(m => (m.Player1Id == userId || m.Player2Id == userId)
                     && m.LeagueSeason.ClubId == clubId
                     && m.PlayedDate.HasValue
                     && m.PlayedDate.Value.Year == year
                     && m.Status == MatchStatus.Completed)
            .ToListAsync(ct);

        stats.LeagueMatchesPlayed = leagueMatches.Count;
        stats.LeagueMatchesWon = leagueMatches.Count(m => m.WinnerId == userId);

        await _db.SaveChangesAsync(ct);
    }

    public async Task RecalculateAllClubStatsAsync(int clubId, int year, CancellationToken ct = default)
    {
        var memberIds = await _db.ClubMemberships
            .Where(m => m.ClubId == clubId && m.IsActive)
            .Select(m => m.UserId)
            .Distinct()
            .ToListAsync(ct);

        foreach (var userId in memberIds)
            await RecalculatePlayerSeasonStatsAsync(userId, clubId, year, ct);
    }

    public async Task UpdateRoundSummaryAsync(int roundId, CancellationToken ct = default)
    {
        var round = await _db.Rounds
            .Include(r => r.HoleScores)
            .Include(r => r.Course).ThenInclude(c => c.Holes)
            .FirstOrDefaultAsync(r => r.Id == roundId, ct);

        if (round == null || !round.HoleScores.Any()) return;

        round.GrossScore = round.HoleScores.Sum(h => h.Strokes);
        round.TotalPutts = round.HoleScores.Where(h => h.Putts.HasValue).Sum(h => h.Putts);
        round.Eagles = round.HoleScores.Count(h => h.ScoreToPar <= -2);
        round.Birdies = round.HoleScores.Count(h => h.ScoreToPar == -1);
        round.Pars = round.HoleScores.Count(h => h.ScoreToPar == 0);
        round.Bogeys = round.HoleScores.Count(h => h.ScoreToPar == 1);
        round.DoubleBogeys = round.HoleScores.Count(h => h.ScoreToPar == 2);
        round.TriplePlusBogeys = round.HoleScores.Count(h => h.ScoreToPar >= 3);
        round.HoleInOnes = round.HoleScores.Count(h => h.IsHoleInOne);
        round.StablefordPoints = round.HoleScores.Sum(h => h.StablefordPoints);

        var fairwayHoles = round.HoleScores.Where(h => h.FairwayHit.HasValue).ToList();
        if (fairwayHoles.Any())
        {
            round.FairwaysHit = fairwayHoles.Count(h => h.FairwayHit == true);
            round.TotalFairways = fairwayHoles.Count;
        }

        var girHoles = round.HoleScores.Where(h => h.GreenInRegulation.HasValue).ToList();
        if (girHoles.Any())
        {
            round.GreensInRegulation = girHoles.Count(h => h.GreenInRegulation == true);
            round.TotalGreens = girHoles.Count;
        }

        var drives = round.HoleScores.Where(h => h.DriveDistanceYards.HasValue).ToList();
        if (drives.Any())
            round.LongestDriveYards = drives.Max(h => h.DriveDistanceYards);

        // Calculate net score if handicap used
        if (round.HandicapUsed.HasValue && round.GrossScore.HasValue)
        {
            var course = round.Course;
            if (course.CourseRating.HasValue && course.SlopeRating.HasValue)
            {
                var courseHandicap = (int)Math.Round(
                    round.HandicapUsed.Value * (course.SlopeRating.Value / 113.0)
                    + (course.CourseRating.Value - course.ParTotal));
                round.NetScore = round.GrossScore.Value - courseHandicap;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateLeagueStandingsAsync(int leagueSeasonId, CancellationToken ct = default)
    {
        var season = await _db.LeagueSeasons
            .Include(l => l.Entries)
            .FirstOrDefaultAsync(l => l.Id == leagueSeasonId, ct);
        if (season == null) return;

        var completedMatches = await _db.LeagueMatches
            .Where(m => m.LeagueSeasonId == leagueSeasonId && m.Status == MatchStatus.Completed)
            .ToListAsync(ct);

        var playerIds = season.Entries.Select(e => e.UserId).Distinct().ToList();
        var existingStandings = await _db.LeagueStandings
            .Where(s => s.LeagueSeasonId == leagueSeasonId)
            .ToListAsync(ct);

        foreach (var userId in playerIds)
        {
            var standing = existingStandings.FirstOrDefault(s => s.UserId == userId)
                ?? new LeagueStanding { LeagueSeasonId = leagueSeasonId, UserId = userId };

            var playerMatches = completedMatches.Where(m => m.Player1Id == userId || m.Player2Id == userId).ToList();
            standing.Played = playerMatches.Count;
            standing.Wins = playerMatches.Count(m => m.WinnerId == userId);
            standing.Losses = playerMatches.Count(m => !m.IsDraw && m.WinnerId != null && m.WinnerId != userId);
            standing.Draws = playerMatches.Count(m => m.IsDraw);
            standing.Points = standing.Wins * season.PointsForWin
                            + standing.Draws * season.PointsForDraw
                            + standing.Losses * season.PointsForLoss;

            var scores = playerMatches
                .Select(m => m.Player1Id == userId ? m.Player1GrossScore : m.Player2GrossScore)
                .Where(s => s.HasValue).Select(s => s!.Value).ToList();

            if (scores.Any())
            {
                standing.TotalStrokes = scores.Sum();
                standing.AverageScore = Math.Round(scores.Average(), 1);
                standing.BestScore = scores.Min();
            }

            if (standing.Id == 0) _db.LeagueStandings.Add(standing);
        }

        await _db.SaveChangesAsync(ct);

        // Recalculate ranks
        var standings = await _db.LeagueStandings
            .Where(s => s.LeagueSeasonId == leagueSeasonId)
            .OrderByDescending(s => s.Points)
            .ThenBy(s => s.AverageScore)
            .ToListAsync(ct);

        for (int i = 0; i < standings.Count; i++)
            standings[i].Rank = i + 1;

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default)
    {
        var entries = await _db.TournamentEntries
            .Where(e => e.TournamentId == tournamentId && e.Status != EntryStatus.Withdrawn && e.Status != EntryStatus.Disqualified)
            .ToListAsync(ct);

        var rounds = await _db.Rounds
            .Where(r => r.Event.TournamentId == tournamentId && r.Status == RoundStatus.Completed)
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, TotalGross = g.Sum(r => r.GrossScore ?? 0), TotalNet = g.Sum(r => r.NetScore ?? 0), TotalStableford = g.Sum(r => r.StablefordPoints ?? 0) })
            .ToListAsync(ct);

        foreach (var entry in entries)
        {
            var playerRounds = rounds.FirstOrDefault(r => r.UserId == entry.UserId);
            if (playerRounds != null)
            {
                entry.TotalGrossScore = playerRounds.TotalGross;
                entry.TotalNetScore = playerRounds.TotalNet;
                entry.TotalStablefordPoints = playerRounds.TotalStableford;
            }
        }

        // Rank by gross score (ascending = better)
        var ranked = entries
            .Where(e => e.TotalGrossScore.HasValue)
            .OrderBy(e => e.TotalGrossScore)
            .ToList();

        for (int i = 0; i < ranked.Count; i++)
            ranked[i].FinalPosition = i + 1;

        await _db.SaveChangesAsync(ct);
    }
}
