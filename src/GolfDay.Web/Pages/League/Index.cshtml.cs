using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.League;

public class LeagueIndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public LeagueIndexModel(IApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true, Name = "filter")]
    public string Filter { get; set; } = string.Empty;

    public List<LeagueSeason> Leagues { get; set; } = new();

    public int AllCount          { get; set; }
    public int ActiveCount       { get; set; }
    public int RegistrationCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var all = await _db.LeagueSeasons
            .Include(l => l.Club)
            .Include(l => l.Entries)
            .Include(l => l.Matches)
            .Where(l => l.Status != LeagueStatus.Cancelled)
            .OrderByDescending(l => l.Status == LeagueStatus.Active)
            .ThenByDescending(l => l.StartDate)
            .ToListAsync();

        AllCount          = all.Count;
        ActiveCount       = all.Count(l => l.Status == LeagueStatus.Active);
        RegistrationCount = all.Count(l => l.Status == LeagueStatus.Registration);

        Leagues = Filter?.ToLower() switch
        {
            "active"       => all.Where(l => l.Status == LeagueStatus.Active).ToList(),
            "registration" => all.Where(l => l.Status == LeagueStatus.Registration).ToList(),
            "completed"    => all.Where(l => l.Status == LeagueStatus.Completed)
                                 .OrderByDescending(l => l.EndDate).ToList(),
            _              => all
        };

        return Page();
    }
}
