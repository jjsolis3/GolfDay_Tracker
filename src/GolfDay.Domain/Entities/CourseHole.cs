using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class CourseHole : BaseEntity
{
    public int CourseId { get; set; }
    public int HoleNumber { get; set; }
    public int Par { get; set; } = 4;
    public int HandicapIndex { get; set; }
    public int? YardsChampionship { get; set; }
    public int? YardsBack { get; set; }
    public int? YardsMidBack { get; set; }
    public int? YardsMid { get; set; }
    public int? YardsFront { get; set; }
    public int? YardsForward { get; set; }
    public string? Description { get; set; }

    // Navigation
    public GolfCourse Course { get; set; } = null!;
}
