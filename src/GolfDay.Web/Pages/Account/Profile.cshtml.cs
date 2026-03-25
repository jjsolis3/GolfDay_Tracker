using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GolfDay.Web.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _db;

    public ProfileModel(UserManager<ApplicationUser> userManager, IApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public ApplicationUser User { get; set; } = null!;
    public List<Round> RecentRounds { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? State { get; set; }

        [Range(0.0, 54.0)]
        [Display(Name = "Handicap Index")]
        public double? HandicapIndex { get; set; }

        [MaxLength(20)]
        [Display(Name = "GHIN Number")]
        public string? GhinNumber { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        User = await _db.ClubMemberships
            .Include(m => m.Club)
            .Where(m => m.UserId == user.Id && m.IsActive)
            .Select(m => m.User)
            .Include(u => u.ClubMemberships)
            .ThenInclude(m => m.Club)
            .FirstOrDefaultAsync() ?? user;

        if (User.ClubMemberships == null)
        {
            User = user;
            User.ClubMemberships = await _db.ClubMemberships
                .Include(m => m.Club)
                .Where(m => m.UserId == user.Id && m.IsActive)
                .ToListAsync();
        }

        RecentRounds = await _db.Rounds
            .Include(r => r.Event)
            .Include(r => r.Course)
            .Where(r => r.UserId == user.Id && r.Status == RoundStatus.Completed)
            .OrderByDescending(r => r.CompletedAt)
            .Take(10)
            .ToListAsync();

        Input = new InputModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            State = user.State,
            HandicapIndex = user.HandicapIndex,
            GhinNumber = user.GhinNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        if (!ModelState.IsValid)
        {
            User = user;
            return Page();
        }

        user.FirstName = Input.FirstName;
        user.LastName = Input.LastName;
        user.City = Input.City;
        user.State = Input.State;
        user.HandicapIndex = Input.HandicapIndex;
        user.GhinNumber = Input.GhinNumber;

        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToPage();
    }
}
