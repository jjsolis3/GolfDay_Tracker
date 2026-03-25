using GolfDay.Application.Common.Interfaces;
using GolfDay.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GolfDay.Web.Hubs;

/// <summary>
/// Real-time scoring hub. Spectators join event groups to receive live score updates.
/// Scorers post hole scores which broadcast to all connected viewers.
/// </summary>
[Authorize]
public class ScoringHub : Hub
{
    private readonly IApplicationDbContext _db;
    private readonly IStatsService _statsService;
    private readonly ILogger<ScoringHub> _logger;

    public ScoringHub(IApplicationDbContext db, IStatsService statsService, ILogger<ScoringHub> logger)
    {
        _db = db;
        _statsService = statsService;
        _logger = logger;
    }

    /// <summary>Join the live scoring feed for a specific event.</summary>
    public async Task JoinEventGroup(int eventId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"event-{eventId}");
        _logger.LogInformation("User {User} joined scoring group for event {EventId}", Context.UserIdentifier, eventId);
    }

    /// <summary>Leave the event scoring feed.</summary>
    public async Task LeaveEventGroup(int eventId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event-{eventId}");
    }

    /// <summary>Post a hole score and broadcast to all event viewers.</summary>
    public async Task PostHoleScore(int roundId, int holeNumber, int strokes, int? putts, bool? fairwayHit, bool? gir)
    {
        var round = await _db.Rounds.FindAsync(roundId);
        if (round == null) return;

        // Ensure user owns this round
        if (round.UserId != Context.UserIdentifier) return;

        var holeScore = await _db.HoleScores
            .FirstOrDefaultAsync(h => h.RoundId == roundId && h.HoleNumber == holeNumber);

        if (holeScore == null)
        {
            holeScore = new Domain.Entities.HoleScore
            {
                RoundId = roundId,
                HoleNumber = holeNumber,
                Par = await GetHoleParAsync(round.CourseId, holeNumber),
                Strokes = strokes,
                Putts = putts,
                FairwayHit = fairwayHit,
                GreenInRegulation = gir
            };
            _db.HoleScores.Add(holeScore);
        }
        else
        {
            holeScore.Strokes = strokes;
            holeScore.Putts = putts;
            holeScore.FairwayHit = fairwayHit;
            holeScore.GreenInRegulation = gir;
        }

        await _db.SaveChangesAsync(default);
        await _statsService.UpdateRoundSummaryAsync(roundId, default);

        var updatedRound = await _db.Rounds
            .Include(r => r.User)
            .FirstAsync(r => r.Id == roundId);

        // Broadcast to all viewers of this event
        await Clients.Group($"event-{round.EventId}").SendAsync("ScoreUpdated", new
        {
            RoundId = roundId,
            HoleNumber = holeNumber,
            Strokes = strokes,
            ScoreToPar = holeScore.ScoreToPar,
            ScoreLabel = holeScore.ScoreLabel,
            PlayerName = updatedRound.User.FullName,
            GrossTotal = updatedRound.GrossScore,
            HolesCompleted = await _db.HoleScores.CountAsync(h => h.RoundId == roundId),
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>Mark a round as complete.</summary>
    public async Task CompleteRound(int roundId)
    {
        var round = await _db.Rounds.FindAsync(roundId);
        if (round == null || round.UserId != Context.UserIdentifier) return;

        round.Status = Domain.Enums.RoundStatus.Completed;
        round.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(default);
        await _statsService.UpdateRoundSummaryAsync(roundId, default);

        await Clients.Group($"event-{round.EventId}").SendAsync("RoundCompleted", new
        {
            RoundId = roundId,
            UserId = round.UserId,
            GrossScore = round.GrossScore
        });
    }

    private async Task<int> GetHoleParAsync(int courseId, int holeNumber)
    {
        var hole = await _db.CourseHoles
            .FirstOrDefaultAsync(h => h.CourseId == courseId && h.HoleNumber == holeNumber);
        return hole?.Par ?? 4;
    }

    // EF helper shorthand since we can't use LINQ extensions without using statement in hub
    private async Task<bool> AnyAsync(IQueryable<Domain.Entities.HoleScore> query)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(query);
    }
    private async Task<Domain.Entities.HoleScore?> FirstOrDefaultAsync(IQueryable<Domain.Entities.HoleScore> query)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(query);
    }
    private async Task<int> CountAsync(IQueryable<Domain.Entities.HoleScore> query)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
    }
    private async Task<Domain.Entities.Round> FirstAsync(IQueryable<Domain.Entities.Round> query)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstAsync(query);
    }
}
