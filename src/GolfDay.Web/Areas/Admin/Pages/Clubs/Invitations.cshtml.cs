using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Clubs;

[Authorize(Policy = "AdminPolicy")]
public class InvitationsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvitationsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }   // ClubId

    public Club? Club { get; set; }
    public List<ClubMembershipInvitation> Invitations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();

        Invitations = await _db.ClubMembershipInvitations
            .Include(i => i.InvitedBy)
            .Where(i => i.ClubId == Id)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCreateInviteAsync(
        string email, int expiryDays)
    {
        var managerId = _userManager.GetUserId(User)!;

        var invite = new ClubMembershipInvitation
        {
            ClubId = Id,
            InvitedByUserId = managerId,
            Token = GenerateToken(),
            Email = email.Trim().ToLower(),
            IsJoinCode = false,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays > 0 ? expiryDays : 7)
        };
        _db.ClubMembershipInvitations.Add(invite);
        await _db.SaveChangesAsync();

        var link = Url.Page("/Club/Join", null, new { token = invite.Token }, Request.Scheme);
        TempData["SuccessMessage"] = $"Invite created. Share this link with {email}: {link}";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostCreateJoinCodeAsync(
        string label, int? maxUses, int expiryDays)
    {
        var managerId = _userManager.GetUserId(User)!;

        var code = new ClubMembershipInvitation
        {
            ClubId = Id,
            InvitedByUserId = managerId,
            Token = GenerateShortCode(),
            IsJoinCode = true,
            Label = label.Trim(),
            MaxUses = maxUses > 0 ? maxUses : null,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays > 0 ? expiryDays : 30)
        };
        _db.ClubMembershipInvitations.Add(code);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Join code '{code.Token}' created. Share it with new members.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostRevokeAsync(int inviteId)
    {
        var invite = await _db.ClubMembershipInvitations
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.ClubId == Id);

        if (invite != null)
        {
            invite.IsRevoked = true;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Invitation revoked.";
        }

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int inviteId)
    {
        var invite = await _db.ClubMembershipInvitations
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.ClubId == Id);

        if (invite != null)
        {
            _db.ClubMembershipInvitations.Remove(invite);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Invitation deleted.";
        }

        return RedirectToPage(new { id = Id });
    }

    private static string GenerateToken() =>
        Guid.NewGuid().ToString("N");

    private static string GenerateShortCode() =>
        Guid.NewGuid().ToString("N")[..10].ToUpper();
}
