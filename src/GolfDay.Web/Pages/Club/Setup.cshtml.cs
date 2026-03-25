using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClubEntity = GolfDay.Domain.Entities.Club;

namespace GolfDay.Web.Pages.Club;

[Authorize(Policy = "AdminPolicy")]
public class SetupModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public SetupModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int Step { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int ClubId { get; set; }

    // Step 1 inputs
    [BindProperty] public string ClubName { get; set; } = string.Empty;
    [BindProperty] public string? ClubDescription { get; set; }
    [BindProperty] public string? City { get; set; }
    [BindProperty] public string? State { get; set; }
    [BindProperty] public string? ContactEmail { get; set; }
    [BindProperty] public bool IsPublic { get; set; } = true;

    // Step 2 inputs
    [BindProperty] public string CourseName { get; set; } = string.Empty;
    [BindProperty] public double CourseRating { get; set; } = 72.0;
    [BindProperty] public int SlopeRating { get; set; } = 113;
    [BindProperty] public int NumberOfHoles { get; set; } = 18;
    [BindProperty] public int CoursePar { get; set; } = 72;

    // Step 3 inputs
    [BindProperty] public string InviteEmails { get; set; } = string.Empty;

    public ClubEntity? Club { get; set; }
    public GolfCourse? Course { get; set; }

    public async Task OnGetAsync()
    {
        if (ClubId > 0)
        {
            Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId);
            Course = await _db.GolfCourses.FirstOrDefaultAsync(c => c.ClubId == ClubId);
        }
    }

    public async Task<IActionResult> OnPostStep1Async()
    {
        if (string.IsNullOrWhiteSpace(ClubName))
        {
            ModelState.AddModelError(nameof(ClubName), "Club name is required.");
            Step = 1;
            return Page();
        }

        var userId = _userManager.GetUserId(User)!;
        var slug = ClubName.ToLower().Replace(" ", "-").Replace("'", "")
            + "-" + Guid.NewGuid().ToString("N")[..6];

        var club = new ClubEntity
        {
            Name = ClubName.Trim(),
            Slug = slug,
            Description = ClubDescription?.Trim(),
            City = City?.Trim(),
            State = State?.Trim(),
            ContactEmail = ContactEmail?.Trim(),
            IsPublic = IsPublic,
            IsActive = true
        };
        _db.Clubs.Add(club);
        await _db.SaveChangesAsync();

        // Add creator as Admin member
        _db.ClubMemberships.Add(new ClubMembership
        {
            ClubId = club.Id,
            UserId = userId,
            Role = Domain.Enums.ClubRole.Admin,
            MembershipType = Domain.Enums.MembershipType.Full,
            MembershipStatus = Domain.Enums.MembershipStatus.Active,
            IsActive = true,
            JoinedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return RedirectToPage(new { step = 2, clubId = club.Id });
    }

    public async Task<IActionResult> OnPostStep2Async()
    {
        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId);
        if (club is null) return NotFound();

        var course = new GolfCourse
        {
            ClubId = ClubId,
            Name = CourseName.Trim(),
            CourseRating = CourseRating,
            SlopeRating = (double)SlopeRating,
            NumberOfHoles = NumberOfHoles,
            ParTotal = CoursePar,
            IsActive = true
        };
        _db.GolfCourses.Add(course);
        await _db.SaveChangesAsync();

        return RedirectToPage(new { step = 3, clubId = ClubId });
    }

    public async Task<IActionResult> OnPostStep3Async()
    {
        var userId = _userManager.GetUserId(User)!;
        var emails = InviteEmails.Split(new[] { ',', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim()).Where(e => e.Contains('@')).Distinct().Take(20).ToList();

        foreach (var email in emails)
        {
            _db.ClubMembershipInvitations.Add(new ClubMembershipInvitation
            {
                ClubId = ClubId,
                InvitedByUserId = userId,
                Token = Guid.NewGuid().ToString("N"),
                Email = email.ToLower(),
                IsJoinCode = false,
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            });
        }
        if (emails.Any()) await _db.SaveChangesAsync();

        return RedirectToPage(new { step = 4, clubId = ClubId });
    }

    public IActionResult OnPostStep4Async()
    {
        return RedirectToPage(new { step = 5, clubId = ClubId });
    }
}
