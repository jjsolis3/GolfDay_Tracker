using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GolfDay.Web.Pages.Rounds;

[Authorize]
public class EnterModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public EnterModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<GolfCourse> Courses { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Please select a course.")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Required]
        [Display(Name = "Date Played")]
        public DateTime PlayedDate { get; set; } = DateTime.Today;

        [Required, Range(18, 200, ErrorMessage = "Gross score must be between 18 and 200.")]
        [Display(Name = "Gross Score")]
        public int GrossScore { get; set; }

        [Range(18, 200)]
        [Display(Name = "Net Score")]
        public int? NetScore { get; set; }

        [Range(0.0, 54.0)]
        [Display(Name = "Handicap Used")]
        public double? HandicapUsed { get; set; }

        [Range(0, 200)]
        [Display(Name = "Total Putts")]
        public int? TotalPutts { get; set; }

        [Range(0, 18)]
        [Display(Name = "Fairways Hit")]
        public int? FairwaysHit { get; set; }

        [Range(0, 18)]
        [Display(Name = "Greens in Regulation")]
        public int? GreensInRegulation { get; set; }

        [Range(0, 18)]
        [Display(Name = "Birdies")]
        public int? Birdies { get; set; }

        [Range(0, 18)]
        [Display(Name = "Pars")]
        public int? Pars { get; set; }

        [Range(0, 18)]
        [Display(Name = "Bogeys")]
        public int? Bogeys { get; set; }

        [Range(0, 18)]
        [Display(Name = "Double Bogeys+")]
        public int? DoubleBogeys { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        await LoadCoursesAsync(user.Id);
        Input.HandicapUsed = user.HandicapIndex;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(user.Id);
            return Page();
        }

        var course = await _db.GolfCourses
            .Include(c => c.Club)
            .FirstOrDefaultAsync(c => c.Id == Input.CourseId && c.IsActive);

        if (course == null)
        {
            ModelState.AddModelError("Input.CourseId", "Selected course not found.");
            await LoadCoursesAsync(user.Id);
            return Page();
        }

        var playedUtc = DateTime.SpecifyKind(Input.PlayedDate.Date, DateTimeKind.Utc);

        var round = new Round
        {
            EventId      = null,
            CourseId     = Input.CourseId,
            UserId       = user.Id,
            RoundNumber  = 1,
            Status       = RoundStatus.Completed,
            StartedAt    = playedUtc,
            CompletedAt  = playedUtc.AddHours(4),
            GrossScore   = Input.GrossScore,
            NetScore     = Input.NetScore,
            HandicapUsed = Input.HandicapUsed,
            TotalPutts   = Input.TotalPutts,
            FairwaysHit  = Input.FairwaysHit,
            TotalFairways       = course.NumberOfHoles - 2,  // par-3s don't count
            GreensInRegulation  = Input.GreensInRegulation,
            TotalGreens  = course.NumberOfHoles,
            Birdies      = Input.Birdies,
            Pars         = Input.Pars,
            Bogeys       = Input.Bogeys,
            DoubleBogeys = Input.DoubleBogeys,
            Notes        = Input.Notes?.Trim(),
            IsAttested   = false
        };

        _db.Rounds.Add(round);
        await _db.SaveChangesAsync();

        var netStr = Input.NetScore.HasValue ? $" / Net {Input.NetScore}" : string.Empty;
        TempData["SuccessMessage"] =
            $"Round recorded — Gross {Input.GrossScore}{netStr} at {course.Name} on {Input.PlayedDate:MMM d, yyyy}.";

        return RedirectToPage("/Player/Rounds");
    }

    private async Task LoadCoursesAsync(string userId)
    {
        var memberClubIds = await _db.ClubMemberships
            .Where(m => m.UserId == userId && m.IsActive)
            .Select(m => m.ClubId)
            .ToListAsync();

        Courses = await _db.GolfCourses
            .Include(c => c.Club)
            .Where(c => c.IsActive && (memberClubIds.Contains(c.ClubId) || c.Club.IsPublic))
            .OrderBy(c => memberClubIds.Contains(c.ClubId) ? 0 : 1)
            .ThenBy(c => c.Club.Name)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }
}
