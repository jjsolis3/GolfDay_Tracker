using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class ClubMembership : BaseEntity
{
    public int ClubId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ClubRole Role { get; set; } = ClubRole.Member;
    public string? MemberNumber { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
