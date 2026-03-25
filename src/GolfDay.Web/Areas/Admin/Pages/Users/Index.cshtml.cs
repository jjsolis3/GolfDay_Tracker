using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Users;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<ApplicationUser> Users { get; set; } = new();
    public List<Club> Clubs { get; set; } = new();
    public Dictionary<string, List<ClubMembership>> UserMemberships { get; set; } = new();
    public Dictionary<string, List<string>> UserRoles { get; set; } = new();
    public int TotalUsers { get; set; }
    public int TotalActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ClubFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ShowAll { get; set; }

    public async Task OnGetAsync()
    {
        Clubs = await _db.Clubs
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var userQuery = _userManager.Users.AsQueryable();

        TotalUsers = await userQuery.CountAsync();
        TotalActive = await userQuery.CountAsync(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            userQuery = userQuery.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(s)));
        }

        if (StatusFilter == "active")
            userQuery = userQuery.Where(u => u.IsActive);
        else if (StatusFilter == "inactive")
            userQuery = userQuery.Where(u => !u.IsActive);

        if (ClubFilter.HasValue)
        {
            var clubMemberIds = await _db.ClubMemberships
                .Where(m => m.ClubId == ClubFilter.Value && m.IsActive)
                .Select(m => m.UserId)
                .ToListAsync();
            userQuery = userQuery.Where(u => clubMemberIds.Contains(u.Id));
        }

        var pageSize = ShowAll ? int.MaxValue : 50;

        Users = await userQuery
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Take(pageSize)
            .ToListAsync();

        // Load memberships for these users
        var userIds = Users.Select(u => u.Id).ToList();

        var memberships = await _db.ClubMemberships
            .Where(m => userIds.Contains(m.UserId) && m.IsActive)
            .Include(m => m.Club)
            .ToListAsync();

        UserMemberships = memberships
            .GroupBy(m => m.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load roles for these users
        foreach (var user in Users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
                UserRoles[user.Id] = roles.ToList();
        }
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = user.IsActive
            ? $"{user.FullName} has been activated."
            : $"{user.FullName} has been deactivated.";

        return RedirectToPage(new
        {
            search = Search,
            statusFilter = StatusFilter,
            clubFilter = ClubFilter
        });
    }
}
