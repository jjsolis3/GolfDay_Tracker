using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

public class ClubMembership : BaseEntity
{
    public int ClubId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // ── Role & Type ──────────────────────────────────────────────────────────
    public ClubRole Role { get; set; } = ClubRole.Member;
    public MembershipType MembershipType { get; set; } = MembershipType.Full;
    public MembershipStatus MembershipStatus { get; set; } = MembershipStatus.PendingApproval;

    // ── Identity ─────────────────────────────────────────────────────────────
    public string? MemberNumber { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }

    /// <summary>Legacy / convenience flag derived from MembershipStatus == Active.</summary>
    public bool IsActive { get; set; } = false;

    public string? Notes { get; set; }

    // ── How they joined ───────────────────────────────────────────────────────
    /// <summary>FK to the invitation/join-code that granted access, if any.</summary>
    public int? InvitationId { get; set; }

    /// <summary>FK to the approved join request, if they came via request flow.</summary>
    public int? JoinRequestId { get; set; }

    // ── Access control (overrides per-member if needed) ───────────────────────
    public bool CanEnterTournaments { get; set; } = true;
    public bool CanEnterLeagues { get; set; } = true;
    public bool CanBookTeeTime { get; set; } = true;

    // ── Dues & Payment tracking ───────────────────────────────────────────────
    public decimal? AnnualDueAmount { get; set; }

    /// <summary>Membership is paid through this date. Null = never paid / complimentary.</summary>
    public DateTime? DuesPaidThrough { get; set; }

    public DateTime? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }

    /// <summary>Free-form payment notes (e.g. "Paid by check #1042").</summary>
    public string? PaymentNotes { get; set; }

    /// <summary>Total lifetime amount paid to this club.</summary>
    public decimal TotalPaid { get; set; } = 0;

    // ── Navigation ────────────────────────────────────────────────────────────
    public Club Club { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public ClubMembershipInvitation? Invitation { get; set; }
    public ClubJoinRequest? JoinRequest { get; set; }
}
