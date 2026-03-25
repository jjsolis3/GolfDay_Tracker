using GolfDay.Domain.Entities;

namespace GolfDay.Application.Services;

public interface IStatsService
{
    Task RecalculatePlayerSeasonStatsAsync(string userId, int clubId, int year, CancellationToken ct = default);
    Task RecalculateAllClubStatsAsync(int clubId, int year, CancellationToken ct = default);
    Task UpdateRoundSummaryAsync(int roundId, CancellationToken ct = default);
    Task UpdateLeagueStandingsAsync(int leagueSeasonId, CancellationToken ct = default);
    Task UpdateTournamentLeaderboardAsync(int tournamentId, CancellationToken ct = default);
}
