using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class LeagueStanding : BaseEntity
{
    public int LeagueSeasonId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Rank { get; set; }
    public int Played { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int Points { get; set; }
    public int? TotalStrokes { get; set; }
    public double? AverageScore { get; set; }
    public int? BestScore { get; set; }

    // Navigation
    public LeagueSeason LeagueSeason { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
