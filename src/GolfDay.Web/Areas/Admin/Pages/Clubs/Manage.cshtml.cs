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
public class ManageModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Club? Club { get; set; }
    public List<ClubMembership> Members { get; set; } = new();
    public List<GolfCourse> Courses { get; set; } = new();
    public int TotalEventCount { get; set; }
    public int TournamentCount { get; set; }
    public int LeagueSeasonCount { get; set; }
    public int PendingJoinRequestCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();

        await LoadDataAsync();
        return Page();
    }

    private async Task LoadDataAsync()
    {
        Members = await _db.ClubMemberships
            .Where(m => m.ClubId == Id)
            .Include(m => m.User)
            .OrderBy(m => m.User.LastName)
            .ThenBy(m => m.User.FirstName)
            .ToListAsync();

        Courses = await _db.GolfCourses
            .Where(c => c.ClubId == Id)
            .OrderBy(c => c.Name)
            .ToListAsync();

        TotalEventCount = await _db.GolfEvents.CountAsync(e => e.ClubId == Id);
        TournamentCount = await _db.Tournaments.CountAsync(t => t.ClubId == Id);
        LeagueSeasonCount = await _db.LeagueSeasons.CountAsync(l => l.ClubId == Id);
        PendingJoinRequestCount = await _db.ClubJoinRequests
            .CountAsync(r => r.ClubId == Id && r.Status == JoinRequestStatus.Pending);
    }

    public async Task<IActionResult> OnPostAddMemberAsync(
        string userEmail, int role, string? memberNumber)
    {
        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (club is null) return NotFound();

        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null)
        {
            TempData["ErrorMessage"] = $"No user found with email '{userEmail}'.";
            return RedirectToPage(new { id = Id });
        }

        var existing = await _db.ClubMemberships
            .FirstOrDefaultAsync(m => m.ClubId == Id && m.UserId == user.Id);

        if (existing is not null)
        {
            TempData["ErrorMessage"] = "This user is already a member of the club.";
            return RedirectToPage(new { id = Id });
        }

        var membership = new ClubMembership
        {
            ClubId = Id,
            UserId = user.Id,
            Role = (ClubRole)role,
            MembershipType = MembershipType.Full,
            MembershipStatus = MembershipStatus.Active,
            MemberNumber = memberNumber,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        _db.ClubMemberships.Add(membership);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{user.FullName} has been added as a club member.";
        return RedirectToPage(new { id = Id, tab = "members" });
    }

    public async Task<IActionResult> OnPostRemoveMemberAsync(int membershipId)
    {
        var membership = await _db.ClubMemberships.FirstOrDefaultAsync(m => m.Id == membershipId);
        if (membership is null || membership.ClubId != Id) return NotFound();

        _db.ClubMemberships.Remove(membership);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Member has been removed from the club.";
        return RedirectToPage(new { id = Id, tab = "members" });
    }

    public async Task<IActionResult> OnPostUpdateRoleAsync(int membershipId, int newRole)
    {
        var membership = await _db.ClubMemberships.FirstOrDefaultAsync(m => m.Id == membershipId);
        if (membership is null || membership.ClubId != Id) return NotFound();

        membership.Role = (ClubRole)newRole;
        membership.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Member role updated successfully.";
        return RedirectToPage(new { id = Id, tab = "members" });
    }

    public async Task<IActionResult> OnPostUpdateSettingsAsync(
        string clubName, string? description, string? contactEmail,
        string? contactPhone, string? website, string? city, string? state,
        string? zipCode, bool isActive, bool isPublic)
    {
        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (club is null) return NotFound();

        club.Name = clubName;
        club.Description = description;
        club.ContactEmail = contactEmail;
        club.ContactPhone = contactPhone;
        club.Website = website;
        club.City = city;
        club.State = state;
        club.ZipCode = zipCode;
        club.IsActive = isActive;
        club.IsPublic = isPublic;
        club.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Club settings saved successfully.";
        return RedirectToPage(new { id = Id, tab = "settings" });
    }
}
