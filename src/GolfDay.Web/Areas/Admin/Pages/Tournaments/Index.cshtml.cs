using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Tournaments;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public List<Tournament> Tournaments { get; set; } = new();
    public List<Club> Clubs { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ClubFilter { get; set; }

    public async Task OnGetAsync()
    {
        Clubs = await _db.Clubs
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var query = _db.Tournaments
            .Include(t => t.Club)
            .Include(t => t.Entries)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s) ||
                                     (t.Description != null && t.Description.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter) &&
            Enum.TryParse<TournamentStatus>(StatusFilter, out var statusEnum))
        {
            query = query.Where(t => t.Status == statusEnum);
        }

        if (ClubFilter.HasValue)
            query = query.Where(t => t.ClubId == ClubFilter.Value);

        Tournaments = await query
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }
}
