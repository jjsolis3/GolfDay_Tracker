using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.League;

public class LeagueIndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public LeagueIndexModel(IApplicationDbContext db) => _db = db;

    public List<LeagueSeason> Leagues { get; set; } = new();

    public async Task OnGetAsync()
    {
        Leagues = await _db.LeagueSeasons
            .Include(l => l.Club)
            .Include(l => l.Entries)
            .Include(l => l.Matches)
            .Where(l => l.Status != LeagueStatus.Cancelled)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();
    }
}
