using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

/// <summary>
/// A player's request to join a club, pending manager approval.
/// </summary>
public class ClubJoinRequest : BaseEntity
{
    public int ClubId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;

    /// <summary>Optional message from the applicant to the club manager.</summary>
    public string? ApplicantMessage { get; set; }

    /// <summary>Manager's notes when approving or rejecting.</summary>
    public string? ReviewNotes { get; set; }

    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ApplicationUser? ReviewedBy { get; set; }
}
