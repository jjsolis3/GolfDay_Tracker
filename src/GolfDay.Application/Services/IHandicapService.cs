namespace GolfDay.Application.Services;

public interface IHandicapService
{
    /// <summary>
    /// Calculate the World Handicap System course handicap.
    /// Course Handicap = Handicap Index × (Slope Rating / 113) + (Course Rating - Par)
    /// </summary>
    int CalculateCourseHandicap(double handicapIndex, double slopeRating, double courseRating, int par);

    /// <summary>
    /// Calculate net score given gross score and course handicap.
    /// </summary>
    int CalculateNetScore(int grossScore, int courseHandicap);

    /// <summary>
    /// Suggest updated handicap index based on recent rounds (simplified WHS model).
    /// </summary>
    Task<double?> CalculateUpdatedHandicapIndexAsync(string userId, CancellationToken ct = default);
}
