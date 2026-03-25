using GolfDay.Domain.Common;

namespace GolfDay.Domain.Entities;

/// <summary>
/// Represents either a personal email invite OR a reusable join code.
/// - Personal invite: Email is set, IsJoinCode = false, single use
/// - Join code:       Email is null, IsJoinCode = true, MaxUses controls reuse
/// </summary>
public class ClubMembershipInvitation : BaseEntity
{
    public int ClubId { get; set; }
    public string InvitedByUserId { get; set; } = string.Empty;

    /// <summary>Unique random token embedded in the invite link / join code.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Set for personal email invites; null for generic join codes.</summary>
    public string? Email { get; set; }

    /// <summary>True when this is a reusable join code (not a personal invite).</summary>
    public bool IsJoinCode { get; set; } = false;

    /// <summary>Friendly label shown in admin UI, e.g. "Spring 2025 Members".</summary>
    public string? Label { get; set; }

    /// <summary>Max times a join code can be used. Null = unlimited.</summary>
    public int? MaxUses { get; set; }

    public int UseCount { get; set; } = 0;

    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;

    // Navigation
    public Club Club { get; set; } = null!;
    public ApplicationUser InvitedBy { get; set; } = null!;
    public ICollection<ClubMembership> ResultingMemberships { get; set; } = new List<ClubMembership>();
}
