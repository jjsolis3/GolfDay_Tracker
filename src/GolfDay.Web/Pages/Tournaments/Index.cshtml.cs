using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Tournaments;

public class TournamentsIndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public TournamentsIndexModel(IApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true, Name = "filter")]
    public string Filter { get; set; } = string.Empty;

    public List<Tournament> Tournaments { get; set; } = new();

    public int AllCount          { get; set; }
    public int RegistrationCount { get; set; }
    public int ActiveCount       { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var all = await _db.Tournaments
            .Include(t => t.Club)
            .Include(t => t.Course)
            .Include(t => t.Entries)
            .Where(t => t.IsPublic && t.Status != TournamentStatus.Cancelled)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();

        AllCount          = all.Count;
        RegistrationCount = all.Count(t => t.Status == TournamentStatus.Registration);
        ActiveCount       = all.Count(t => t.Status == TournamentStatus.Active);

        Tournaments = Filter?.ToLower() switch
        {
            "registration" => all.Where(t => t.Status == TournamentStatus.Registration).ToList(),
            "active"       => all.Where(t => t.Status == TournamentStatus.Active).ToList(),
            "completed"    => all.Where(t => t.Status == TournamentStatus.Completed)
                                 .OrderByDescending(t => t.EndDate).ToList(),
            _              => all
        };

        return Page();
    }
}
