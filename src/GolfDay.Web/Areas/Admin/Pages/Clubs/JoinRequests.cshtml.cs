using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Clubs;

[Authorize(Policy = "AdminPolicy")]
public class JoinRequestsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public JoinRequestsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }  // ClubId

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "pending";

    public Club? Club { get; set; }
    public List<ClubJoinRequest> Requests { get; set; } = new();
    public int PendingCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();

        PendingCount = await _db.ClubJoinRequests
            .CountAsync(r => r.ClubId == Id && r.Status == JoinRequestStatus.Pending);

        var query = _db.ClubJoinRequests
            .Include(r => r.User)
            .Include(r => r.ReviewedBy)
            .Where(r => r.ClubId == Id);

        query = Filter switch
        {
            "approved" => query.Where(r => r.Status == JoinRequestStatus.Approved),
            "rejected" => query.Where(r => r.Status == JoinRequestStatus.Rejected),
            _          => query.Where(r => r.Status == JoinRequestStatus.Pending)
        };

        Requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostApproveAsync(int requestId, string? reviewNotes, string? membershipType)
    {
        var request = await _db.ClubJoinRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ClubId == Id);

        if (request is null) return NotFound();

        var managerId = _userManager.GetUserId(User)!;

        request.Status = JoinRequestStatus.Approved;
        request.ReviewedByUserId = managerId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = reviewNotes?.Trim();

        // Create membership
        var alreadyMember = await _db.ClubMemberships
            .AnyAsync(m => m.ClubId == Id && m.UserId == request.UserId);

        if (!alreadyMember)
        {
            var type = membershipType switch
            {
                "Associate" => MembershipType.Associate,
                "Guest"     => MembershipType.Guest,
                "Honorary"  => MembershipType.Honorary,
                _           => MembershipType.Full
            };

            _db.ClubMemberships.Add(new ClubMembership
            {
                ClubId = Id,
                UserId = request.UserId,
                Role = ClubRole.Member,
                MembershipType = type,
                MembershipStatus = MembershipStatus.Active,
                IsActive = true,
                JoinedAt = DateTime.UtcNow,
                JoinRequestId = request.Id
            });
        }

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{request.User.FullName}'s request has been approved. They are now a member.";
        return RedirectToPage(new { id = Id, filter = "pending" });
    }

    public async Task<IActionResult> OnPostRejectAsync(int requestId, string? reviewNotes)
    {
        var request = await _db.ClubJoinRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ClubId == Id);

        if (request is null) return NotFound();

        var managerId = _userManager.GetUserId(User)!;

        request.Status = JoinRequestStatus.Rejected;
        request.ReviewedByUserId = managerId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ReviewNotes = reviewNotes?.Trim();

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{request.User.FullName}'s request has been rejected.";
        return RedirectToPage(new { id = Id, filter = "pending" });
    }
}
