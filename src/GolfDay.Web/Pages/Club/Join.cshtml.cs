using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClubEntity = GolfDay.Domain.Entities.Club;

namespace GolfDay.Web.Pages.Club;

[Authorize]
public class JoinModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public JoinModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty]
    public int? ClubId { get; set; }

    [BindProperty]
    public string? Message { get; set; }

    [BindProperty]
    public string? JoinCode { get; set; }

    // Resolved from token on GET
    public ClubMembershipInvitation? Invitation { get; set; }
    public ClubEntity? PreselectedClub { get; set; }
    public List<ClubEntity> PublicClubs { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Token))
        {
            Invitation = await _db.ClubMembershipInvitations
                .Include(i => i.Club)
                .FirstOrDefaultAsync(i => i.Token == Token && !i.IsRevoked && i.ExpiresAt > DateTime.UtcNow);

            PreselectedClub = Invitation?.Club;
        }

        PublicClubs = await _db.Clubs
            .Where(c => c.IsActive && c.IsPublic)
            .OrderBy(c => c.Name)
            .Take(50)
            .ToListAsync();
    }

    // Handler: redeem an invite token or join code
    public async Task<IActionResult> OnPostRedeemAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        var code = (JoinCode ?? Token ?? "").Trim();
        if (string.IsNullOrEmpty(code))
        {
            ErrorMessage = "Please enter a valid invite code.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        var invite = await _db.ClubMembershipInvitations
            .Include(i => i.Club)
            .FirstOrDefaultAsync(i => i.Token == code && !i.IsRevoked && i.ExpiresAt > DateTime.UtcNow);

        if (invite == null)
        {
            ErrorMessage = "This invite code is invalid or has expired.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        // Personal invite — check email matches
        if (!invite.IsJoinCode && invite.Email != null &&
            !string.Equals(invite.Email, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "This invite was sent to a different email address.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        // Join code — check max uses
        if (invite.IsJoinCode && invite.MaxUses.HasValue && invite.UseCount >= invite.MaxUses.Value)
        {
            ErrorMessage = "This join code has reached its maximum number of uses.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        // Already a member?
        var existing = await _db.ClubMemberships
            .FirstOrDefaultAsync(m => m.ClubId == invite.ClubId && m.UserId == user.Id);
        if (existing != null)
        {
            ErrorMessage = $"You are already a member of {invite.Club.Name}.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        // Create membership
        var membership = new ClubMembership
        {
            ClubId = invite.ClubId,
            UserId = user.Id,
            Role = ClubRole.Member,
            MembershipType = MembershipType.Full,
            MembershipStatus = MembershipStatus.Active,
            IsActive = true,
            JoinedAt = DateTime.UtcNow,
            InvitationId = invite.Id
        };
        _db.ClubMemberships.Add(membership);

        invite.UseCount++;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Welcome! You have joined {invite.Club.Name}.";
        return RedirectToPage("/Club/Dashboard", new { clubId = invite.ClubId });
    }

    // Handler: submit a join request
    public async Task<IActionResult> OnPostRequestAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        if (!ClubId.HasValue || ClubId.Value == 0)
        {
            ErrorMessage = "Please select a club.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId.Value && c.IsActive);
        if (club == null)
        {
            ErrorMessage = "Club not found.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        // Already a member?
        var existing = await _db.ClubMemberships
            .FirstOrDefaultAsync(m => m.ClubId == ClubId.Value && m.UserId == user.Id);
        if (existing != null)
        {
            ErrorMessage = $"You are already a member of {club.Name}.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        // Pending request already exists?
        var pendingRequest = await _db.ClubJoinRequests
            .FirstOrDefaultAsync(r => r.ClubId == ClubId.Value && r.UserId == user.Id
                                   && r.Status == JoinRequestStatus.Pending);
        if (pendingRequest != null)
        {
            ErrorMessage = "You already have a pending request for this club.";
            await PopulatePublicClubsAsync();
            return Page();
        }

        var request = new ClubJoinRequest
        {
            ClubId = ClubId.Value,
            UserId = user.Id,
            Status = JoinRequestStatus.Pending,
            ApplicantMessage = Message?.Trim()
        };
        _db.ClubJoinRequests.Add(request);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Your request to join {club.Name} has been submitted. The club manager will review it shortly.";
        return RedirectToPage("/Index");
    }

    private async Task PopulatePublicClubsAsync()
    {
        PublicClubs = await _db.Clubs
            .Where(c => c.IsActive && c.IsPublic)
            .OrderBy(c => c.Name)
            .Take(50)
            .ToListAsync();
    }
}
