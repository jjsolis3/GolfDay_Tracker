using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Events;

[Authorize(Policy = "AdminPolicy")]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public IndexModel(IApplicationDbContext db) => _db = db;

    public List<GolfEvent> Events { get; set; } = new();
    public Club? FilterClub { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? ClubId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    public async Task OnGetAsync()
    {
        if (ClubId.HasValue)
        {
            FilterClub = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == ClubId.Value);
        }

        var query = _db.GolfEvents
            .Include(e => e.Club)
            .Include(e => e.Participants)
            .AsQueryable();

        if (ClubId.HasValue)
            query = query.Where(e => e.ClubId == ClubId.Value);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(s) ||
                                     (e.Description != null && e.Description.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter) &&
            Enum.TryParse<EventStatus>(StatusFilter, out var statusEnum))
        {
            query = query.Where(e => e.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(TypeFilter) &&
            Enum.TryParse<EventType>(TypeFilter, out var typeEnum))
        {
            query = query.Where(e => e.EventType == typeEnum);
        }

        Events = await query
            .OrderByDescending(e => e.EventDate)
            .ToListAsync();
    }
}
