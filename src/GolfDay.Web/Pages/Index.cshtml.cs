using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public List<GolfEvent> RecentEvents { get; set; } = new();

    public async Task OnGetAsync()
    {
        RecentEvents = await _db.GolfEvents
            .Include(e => e.Club)
            .Include(e => e.Course)
            .Where(e => e.IsPublic
                     && (e.Status == EventStatus.Scheduled || e.Status == EventStatus.InProgress)
                     && e.EventDate >= DateTime.UtcNow.AddDays(-1))
            .OrderBy(e => e.EventDate)
            .Take(6)
            .ToListAsync();
    }
}
