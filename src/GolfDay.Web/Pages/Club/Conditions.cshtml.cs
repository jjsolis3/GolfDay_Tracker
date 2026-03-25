using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Club;

[Authorize]
public class ConditionsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ConditionsModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    { _db = db; _userManager = userManager; }

    [BindProperty(SupportsGet = true)] public int ClubId { get; set; }

    public GolfDay.Domain.Entities.Club? Club { get; set; }
    public List<GolfCourse> Courses { get; set; } = new();
    public List<CourseConditionReport> Reports { get; set; } = new();
    public CourseConditionReport? OfficialReport { get; set; }

    [BindProperty] public int SelectedCourseId { get; set; }
    [BindProperty] public int FairwayConditionVal { get; set; }
    [BindProperty] public int GreenSpeedVal { get; set; }
    [BindProperty] public string? PinPositions { get; set; }
    [BindProperty] public string? Notes { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId);
        if (Club is null) return NotFound();

        Courses = await _db.GolfCourses
            .Where(c => c.ClubId == ClubId && c.IsActive)
            .ToListAsync();

        Reports = await _db.CourseConditionReports
            .Include(r => r.ReportedBy)
            .Include(r => r.Course)
            .Where(r => r.ClubId == ClubId && r.ReportedAt >= DateTime.UtcNow.AddDays(-3))
            .OrderByDescending(r => r.IsOfficialReport)
            .ThenByDescending(r => r.ReportedAt)
            .ToListAsync();

        OfficialReport = Reports.FirstOrDefault(r => r.IsOfficialReport);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = _userManager.GetUserId(User)!;

        var course = await _db.GolfCourses
            .FirstOrDefaultAsync(c => c.Id == SelectedCourseId && c.ClubId == ClubId);
        if (course is null) return NotFound();

        var isMember = await _db.ClubMemberships
            .AnyAsync(m => m.ClubId == ClubId && m.UserId == userId && m.IsActive);
        if (!isMember) return Forbid();

        var isManager = await _db.ClubMemberships
            .AnyAsync(m => m.ClubId == ClubId && m.UserId == userId && m.IsActive &&
                           (m.Role == ClubRole.Owner || m.Role == ClubRole.Manager));

        _db.CourseConditionReports.Add(new CourseConditionReport
        {
            ClubId = ClubId,
            CourseId = SelectedCourseId,
            ReportedByUserId = userId,
            FairwayCondition = (FairwayCondition)FairwayConditionVal,
            GreenSpeed = (GreenSpeed)GreenSpeedVal,
            PinPositions = PinPositions?.Trim(),
            Notes = Notes?.Trim(),
            IsOfficialReport = isManager,
            ReportedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Condition report submitted. Thank you!";
        return RedirectToPage(new { clubId = ClubId });
    }
}
