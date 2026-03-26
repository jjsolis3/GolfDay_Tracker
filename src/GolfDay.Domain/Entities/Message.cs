using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

public class Message : BaseEntity
{
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public int LeagueMatchId { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }

    // Navigation
    public ApplicationUser FromUser { get; set; } = null!;
    public ApplicationUser ToUser { get; set; } = null!;
    public LeagueMatch LeagueMatch { get; set; } = null!;
}
