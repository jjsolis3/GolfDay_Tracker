using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Clubs;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public List<Club> Clubs { get; set; } = new();
    public Dictionary<int, int> MemberCounts { get; set; } = new();
    public Dictionary<int, int> ActiveEventCounts { get; set; } = new();
    public Dictionary<int, int> CourseCounts { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.Clubs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.City != null && c.City.ToLower().Contains(s)) ||
                (c.State != null && c.State.ToLower().Contains(s)));
        }

        if (StatusFilter == "active")
            query = query.Where(c => c.IsActive);
        else if (StatusFilter == "inactive")
            query = query.Where(c => !c.IsActive);

        Clubs = await query.OrderBy(c => c.Name).ToListAsync();

        var clubIds = Clubs.Select(c => c.Id).ToList();

        MemberCounts = await _db.ClubMemberships
            .Where(m => clubIds.Contains(m.ClubId) && m.IsActive)
            .GroupBy(m => m.ClubId)
            .Select(g => new { ClubId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubId, x => x.Count);

        ActiveEventCounts = await _db.GolfEvents
            .Where(e => clubIds.Contains(e.ClubId) &&
                        (e.Status == EventStatus.Scheduled || e.Status == EventStatus.InProgress))
            .GroupBy(e => e.ClubId)
            .Select(g => new { ClubId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubId, x => x.Count);

        CourseCounts = await _db.GolfCourses
            .Where(c => clubIds.Contains(c.ClubId) && c.IsActive)
            .GroupBy(c => c.ClubId)
            .Select(g => new { ClubId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubId, x => x.Count);
    }
}
