namespace GolfDay.Domain.Enums;

public enum MembershipType
{
    Guest     = 0,  // Limited access, no tournament entry
    Associate = 1,  // Can enter events, no league play
    Full      = 2,  // Full access to all club activities
    Honorary  = 3   // Complimentary full access
}
