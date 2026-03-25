using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class HoleScore : BaseEntity
{
    public int RoundId { get; set; }
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int Strokes { get; set; }
    public int? Putts { get; set; }
    public bool? FairwayHit { get; set; }
    public bool? GreenInRegulation { get; set; }
    public bool? SandSave { get; set; }
    public int? PenaltyStrokes { get; set; }
    public int? DriveDistanceYards { get; set; }
    public int? ClosestToPinFeet { get; set; }
    public int? ClosestToPinInches { get; set; }
    public bool IsHoleInOne { get; set; } = false;
    public string? Notes { get; set; }

    // Computed properties
    public int ScoreToPar => Strokes - Par;
    public string ScoreLabel => ScoreToPar switch
    {
        <= -3 => "Albatross",
        -2 => "Eagle",
        -1 => "Birdie",
        0 => "Par",
        1 => "Bogey",
        2 => "Double Bogey",
        3 => "Triple Bogey",
        _ => $"+{ScoreToPar}"
    };
    public int StablefordPoints => Math.Max(0, 2 - ScoreToPar);

    // Navigation
    public Round Round { get; set; } = null!;
}
