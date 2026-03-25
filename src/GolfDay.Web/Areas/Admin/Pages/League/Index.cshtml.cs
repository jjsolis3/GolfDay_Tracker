using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.League;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public List<LeagueSeason> Seasons { get; set; } = new();
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

        var query = _db.LeagueSeasons
            .Include(s => s.Club)
            .Include(s => s.Entries)
            .Include(s => s.Matches)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(s) ||
                                     (l.Description != null && l.Description.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter) &&
            Enum.TryParse<LeagueStatus>(StatusFilter, out var statusEnum))
        {
            query = query.Where(l => l.Status == statusEnum);
        }

        if (ClubFilter.HasValue)
            query = query.Where(l => l.ClubId == ClubFilter.Value);

        Seasons = await query
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }
}
