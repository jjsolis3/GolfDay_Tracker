using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GolfDay.Web.Areas.Admin.Pages.Tournaments;

[Authorize(Policy = "AdminPolicy")]
public class CreateTournamentModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public CreateTournamentModel(IApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ClubSelectList { get; set; } = null!;
    public SelectList CourseSelectList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        [Required]
        public int ClubId { get; set; }
        public int? CourseId { get; set; }
        public TournamentFormat Format { get; set; } = TournamentFormat.StrokePlay;
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today;
        public DateTime? RegistrationDeadline { get; set; }
        [Range(1, 4)]
        public int NumberOfRounds { get; set; } = 1;
        [Range(2, 500)]
        public int MaxParticipants { get; set; } = 100;
        public bool UseHandicaps { get; set; } = true;
        public bool IsPublic { get; set; } = false;
        [MaxLength(5000)]
        public string? Rules { get; set; }
        [MaxLength(2000)]
        public string? Prizes { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        var tournament = new Tournament
        {
            ClubId = Input.ClubId,
            CourseId = Input.CourseId,
            Name = Input.Name,
            Description = Input.Description,
            Format = Input.Format,
            StartDate = Input.StartDate.ToUniversalTime(),
            EndDate = Input.EndDate.ToUniversalTime(),
            RegistrationDeadline = Input.RegistrationDeadline?.ToUniversalTime(),
            NumberOfRounds = Input.NumberOfRounds,
            MaxParticipants = Input.MaxParticipants,
            UseHandicaps = Input.UseHandicaps,
            IsPublic = Input.IsPublic,
            Rules = Input.Rules,
            Prizes = Input.Prizes,
            Status = TournamentStatus.Registration
        };

        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Tournament '{tournament.Name}' created successfully.";
        return RedirectToPage("/Tournaments/Index");
    }

    private async Task LoadSelectListsAsync()
    {
        var clubs = await _db.Clubs.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        ClubSelectList = new SelectList(clubs, "Id", "Name");

        var courses = await _db.GolfCourses.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        CourseSelectList = new SelectList(courses, "Id", "Name");
    }
}
