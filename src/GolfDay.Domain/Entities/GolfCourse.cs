using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class GolfCourse : BaseEntity
{
    /// <summary>Optional managing club. Null = public / standalone course.</summary>
    public int? ClubId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } = "USA";
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public int NumberOfHoles { get; set; } = 18;
    public int ParTotal { get; set; } = 72;
    public double? CourseRating { get; set; }
    public double? SlopeRating { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>When true the course is visible to all clubs, not just the managing club.</summary>
    public bool IsPublic { get; set; } = true;

    // Navigation
    public Club? Club { get; set; }
    public ICollection<CourseHole>       Holes    { get; set; } = new List<CourseHole>();
    public ICollection<GolfCourseTeeBox> TeeBoxes { get; set; } = new List<GolfCourseTeeBox>();
    public ICollection<GolfEvent>        Events   { get; set; } = new List<GolfEvent>();
    public ICollection<Round>            Rounds   { get; set; } = new List<Round>();
}
