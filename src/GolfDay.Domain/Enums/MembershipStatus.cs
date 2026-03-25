namespace GolfDay.Domain.Enums;

public enum MembershipStatus
{
    PendingApproval = 0,  // Submitted join request, awaiting review
    Active          = 1,  // Full access
    Suspended       = 2,  // Temporarily blocked by manager
    Expired         = 3,  // Dues not paid / membership lapsed
    PendingRenewal  = 4   // Active but dues due within 30 days
}
