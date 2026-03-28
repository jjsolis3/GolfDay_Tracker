using System.ComponentModel.DataAnnotations;
using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Events;

[Authorize(Policy = "AdminPolicy")]
public class CreateModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public CreateModel(IApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ClubSelectList { get; set; } = null!;
    public SelectList CourseSelectList { get; set; } = null!;

    public class InputModel
    {
        [Required]
        public int ClubId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public EventType EventType { get; set; } = EventType.DayEvent;

        [Required]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

        public DateTime? EndDate { get; set; }

        public int? CourseId { get; set; }

        public string? Format { get; set; }

        [Range(1, 999)]
        public int MaxParticipants { get; set; } = 100;

        public bool IsPublic { get; set; } = false;

        public bool UseHandicaps { get; set; } = true;
    }

    public async Task OnGetAsync(int? clubId)
    {
        if (clubId.HasValue)
            Input.ClubId = clubId.Value;

        await PopulateSelectListsAsync(Input.ClubId > 0 ? Input.ClubId : null);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(Input.ClubId > 0 ? Input.ClubId : null);
            return Page();
        }

        var club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Input.ClubId);
        if (club is null)
        {
            ModelState.AddModelError("Input.ClubId", "Selected club not found.");
            await PopulateSelectListsAsync(null);
            return Page();
        }

        if (Input.CourseId.HasValue)
        {
            var courseExists = await _db.GolfCourses
                .AnyAsync(c => c.Id == Input.CourseId.Value && c.IsActive);
            if (!courseExists) Input.CourseId = null;
        }

        var golfEvent = new GolfEvent
        {
            ClubId = Input.ClubId,
            CourseId = Input.CourseId,
            Name = Input.Name,
            Description = Input.Description,
            EventType = Input.EventType,
            EventDate = DateTime.SpecifyKind(Input.EventDate, DateTimeKind.Utc),
            EndDate   = Input.EndDate.HasValue
                ? DateTime.SpecifyKind(Input.EndDate.Value, DateTimeKind.Utc)
                : null,
            Format = Input.Format,
            MaxParticipants = Input.MaxParticipants,
            IsPublic = Input.IsPublic,
            UseHandicaps = Input.UseHandicaps,
            Status = EventStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _db.GolfEvents.Add(golfEvent);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Event '{golfEvent.Name}' created successfully.";
        return RedirectToPage("/Events/Index", new { area = "Admin", clubId = Input.ClubId });
    }

    private async Task PopulateSelectListsAsync(int? selectedClubId)
    {
        var clubs = await _db.Clubs
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ClubSelectList = new SelectList(clubs, "Id", "Name", selectedClubId);

        if (selectedClubId.HasValue)
        {
            var courses = await _db.GolfCourses
                .Where(c => c.ClubId == selectedClubId.Value && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            CourseSelectList = new SelectList(courses, "Id", "Name", Input.CourseId);
        }
        else
        {
            CourseSelectList = new SelectList(Enumerable.Empty<GolfCourse>(), "Id", "Name");
        }
    }
}
