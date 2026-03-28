using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

/// <summary>
/// One tee box configuration for a golf course.
/// A single course can have many tee boxes (e.g. Black/Blue/White for men, Red for women).
/// </summary>
public class GolfCourseTeeBox : BaseEntity
{
    public int    GolfCourseId  { get; set; }
    public string TeeBoxName    { get; set; } = string.Empty; // "Black", "Blue", "White", "Red", …
    public string? Gender       { get; set; }                 // "Male" | "Female" | null = all
    public int    Par           { get; set; } = 72;
    public double? CourseRating { get; set; }
    public double? SlopeRating  { get; set; }
    public int?   TotalYards    { get; set; }

    // Navigation
    public GolfCourse GolfCourse { get; set; } = null!;
}
