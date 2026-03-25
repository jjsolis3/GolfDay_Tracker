using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class PlayerSeasonStats : BaseEntity
{
    public int ClubId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int RoundsPlayed { get; set; }
    public int? BestGrossScore { get; set; }
    public int? WorstGrossScore { get; set; }
    public double? AverageGrossScore { get; set; }
    public double? AverageNetScore { get; set; }
    public int TotalBirdiesOrBetter { get; set; }
    public int TotalEagles { get; set; }
    public int TotalHoleInOnes { get; set; }
    public int TotalPars { get; set; }
    public int TotalBogeys { get; set; }
    public int TotalDoubleBogeys { get; set; }
    public int TotalTriplePlusBogeys { get; set; }
    public double? AveragePutts { get; set; }
    public double? FairwayHitPercent { get; set; }
    public double? GIRPercent { get; set; }
    public int? LongestDriveYards { get; set; }
    public double? HandicapIndexEnd { get; set; }
    public int TournamentsPlayed { get; set; }
    public int? BestTournamentPosition { get; set; }
    public int LeagueMatchesPlayed { get; set; }
    public int LeagueMatchesWon { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation
    public Club Club { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
