using GolfDay.Domain.Common;
using GolfDay.Domain.Enums;

namespace GolfDay.Domain.Entities;

/// <summary>Broadcast message from a club manager to members.</summary>
public class ClubAnnouncement : BaseEntity
{
    public int ClubId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string CreatedByUserId { get; set; } = string.Empty;

    public AnnouncementTarget Target { get; set; } = AnnouncementTarget.AllMembers;
    public bool IsPinned { get; set; } = false;
    public bool IsPublished { get; set; } = true;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Navigation
    public Club Club { get; set; } = null!;
    public ApplicationUser CreatedBy { get; set; } = null!;
}
