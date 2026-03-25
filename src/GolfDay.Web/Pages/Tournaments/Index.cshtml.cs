using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Tournaments;

public class TournamentsIndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public TournamentsIndexModel(IApplicationDbContext db) => _db = db;

    public List<Tournament> Tournaments { get; set; } = new();

    public async Task OnGetAsync()
    {
        Tournaments = await _db.Tournaments
            .Include(t => t.Club)
            .Include(t => t.Course)
            .Include(t => t.Entries)
            .Where(t => t.IsPublic && t.Status != TournamentStatus.Cancelled)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
    }
}
