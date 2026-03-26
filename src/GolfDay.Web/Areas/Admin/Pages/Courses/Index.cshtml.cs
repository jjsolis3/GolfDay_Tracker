using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Courses;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public List<GolfCourse> Courses { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? Search   { get; set; }
    [BindProperty(SupportsGet = true)] public string? ClubFilter { get; set; }  // "mine" | "public" | ""
    public List<Club> Clubs { get; set; } = new();

    public async Task OnGetAsync()
    {
        Clubs = await _db.Clubs.OrderBy(c => c.Name).ToListAsync();

        var query = _db.GolfCourses.Include(c => c.Club).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
            query = query.Where(c => c.Name.Contains(Search) ||
                                     (c.City != null && c.City.Contains(Search)) ||
                                     (c.State != null && c.State.Contains(Search)));

        if (ClubFilter == "public")
            query = query.Where(c => c.ClubId == null || c.IsPublic);
        else if (int.TryParse(ClubFilter, out var cid))
            query = query.Where(c => c.ClubId == cid);

        Courses = await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var course = await _db.GolfCourses.FindAsync(id);
        if (course is null) return NotFound();
        course.IsActive = !course.IsActive;
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Course \"{course.Name}\" {(course.IsActive ? "activated" : "deactivated")}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var course = await _db.GolfCourses
            .Include(c => c.Events)
            .Include(c => c.Rounds)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null) return NotFound();

        if (course.Events.Any() || course.Rounds.Any())
        {
            TempData["ErrorMessage"] = "Cannot delete a course that has events or rounds. Deactivate it instead.";
            return RedirectToPage();
        }

        _db.GolfCourses.Remove(course);
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Course \"{course.Name}\" deleted.";
        return RedirectToPage();
    }
}
