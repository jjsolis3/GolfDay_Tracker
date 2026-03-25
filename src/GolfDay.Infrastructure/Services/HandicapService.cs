using GolfDay.Application.Common.Interfaces;
using GolfDay.Application.Services;
using GolfDay.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Infrastructure.Services;

public class HandicapService : IHandicapService
{
    private readonly IApplicationDbContext _db;

    public HandicapService(IApplicationDbContext db) => _db = db;

    public int CalculateCourseHandicap(double handicapIndex, double slopeRating, double courseRating, int par)
    {
        // WHS Formula: Course Handicap = Handicap Index × (Slope Rating / 113) + (Course Rating - Par)
        return (int)Math.Round(handicapIndex * (slopeRating / 113.0) + (courseRating - par));
    }

    public int CalculateNetScore(int grossScore, int courseHandicap) => grossScore - courseHandicap;

    public async Task<double?> CalculateUpdatedHandicapIndexAsync(string userId, CancellationToken ct = default)
    {
        // WHS: Use best 8 of last 20 score differentials
        var recentRounds = await _db.Rounds
            .Include(r => r.Course)
            .Where(r => r.UserId == userId
                     && r.Status == RoundStatus.Completed
                     && r.GrossScore.HasValue
                     && r.Course.CourseRating.HasValue
                     && r.Course.SlopeRating.HasValue)
            .OrderByDescending(r => r.CompletedAt)
            .Take(20)
            .ToListAsync(ct);

        if (recentRounds.Count < 3) return null;

        var differentials = recentRounds
            .Select(r => (r.GrossScore!.Value - r.Course.CourseRating!.Value) * 113.0 / r.Course.SlopeRating!.Value)
            .OrderBy(d => d)
            .ToList();

        int countToUse = recentRounds.Count switch
        {
            3 => 1,
            4 or 5 => 1,
            6 => 2,
            7 or 8 => 2,
            9 or 10 or 11 => 3,
            12 or 13 or 14 => 4,
            15 or 16 => 5,
            17 or 18 => 6,
            19 => 7,
            _ => 8
        };

        var handicapIndex = differentials.Take(countToUse).Average() * 0.96;
        return Math.Round(Math.Min(handicapIndex, 54.0), 1);
    }
}
