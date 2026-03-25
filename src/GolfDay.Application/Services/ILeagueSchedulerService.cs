using GolfDay.Domain.Entities;

namespace GolfDay.Application.Services;

public interface ILeagueSchedulerService
{
    /// <summary>
    /// Generate round-robin schedule for a league season.
    /// Each player faces every other player once.
    /// </summary>
    Task<IEnumerable<LeagueMatch>> GenerateRoundRobinScheduleAsync(int leagueSeasonId, CancellationToken ct = default);

    /// <summary>
    /// Generate single elimination bracket schedule.
    /// </summary>
    Task<IEnumerable<LeagueMatch>> GenerateSingleEliminationBracketAsync(int leagueSeasonId, CancellationToken ct = default);

    /// <summary>
    /// Advance overdue matches to Overdue status.
    /// </summary>
    Task MarkOverdueMatchesAsync(CancellationToken ct = default);

    /// <summary>
    /// Get upcoming matches for a player across all active leagues.
    /// </summary>
    Task<IEnumerable<LeagueMatch>> GetUpcomingMatchesForPlayerAsync(string userId, CancellationToken ct = default);
}
