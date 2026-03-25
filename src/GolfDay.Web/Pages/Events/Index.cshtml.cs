using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Events;

public class EventsIndexModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public EventsIndexModel(IApplicationDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = string.Empty;

    public List<GolfEvent> Events { get; set; } = new();
    public List<GolfEvent> LiveEvents { get; set; } = new();

    public int AllCount { get; set; }
    public int TodayCount { get; set; }
    public int LiveCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var baseQuery = _db.GolfEvents
            .Include(e => e.Club)
            .Include(e => e.Course)
            .Include(e => e.Participants)
            .Where(e => e.IsPublic && e.Status != EventStatus.Draft && e.Status != EventStatus.Cancelled);

        var allEvents = await baseQuery
            .OrderByDescending(e => e.Status == EventStatus.InProgress)
            .ThenBy(e => e.EventDate)
            .ToListAsync();

        AllCount = allEvents.Count;
        TodayCount = allEvents.Count(e => e.EventDate.Date == today);
        LiveCount = allEvents.Count(e => e.Status == EventStatus.InProgress);
        LiveEvents = allEvents.Where(e => e.Status == EventStatus.InProgress).ToList();

        Events = StatusFilter?.ToLower() switch
        {
            "today" => allEvents.Where(e => e.EventDate.Date == today).ToList(),
            "live" => allEvents.Where(e => e.Status == EventStatus.InProgress).ToList(),
            "upcoming" => allEvents.Where(e => e.Status == EventStatus.Scheduled && e.EventDate.Date >= today)
                                   .OrderBy(e => e.EventDate).ToList(),
            "completed" => allEvents.Where(e => e.Status == EventStatus.Completed)
                                    .OrderByDescending(e => e.EventDate).ToList(),
            _ => allEvents
        };

        return Page();
    }
}
