using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GolfDay.Web.Areas.Admin.Pages.League;

[Authorize(Policy = "AdminPolicy")]
public class CreateLeagueModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public CreateLeagueModel(IApplicationDbContext db) => _db = db;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList ClubSelectList { get; set; } = null!;

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        [Required]
        public int ClubId { get; set; }
        public LeagueFormat Format { get; set; } = LeagueFormat.RoundRobin;
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Today;
        [Required]
        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);
        [Range(2, 128)]
        public int MaxParticipants { get; set; } = 32;
        [Range(1, 90)]
        public int MatchDeadlineDays { get; set; } = 14;
        [Range(0, 10)]
        public int PointsForWin { get; set; } = 3;
        [Range(0, 10)]
        public int PointsForDraw { get; set; } = 1;
        [Range(0, 10)]
        public int PointsForLoss { get; set; } = 0;
        public bool UseHandicaps { get; set; } = true;
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

        var league = new LeagueSeason
        {
            ClubId = Input.ClubId,
            Name = Input.Name,
            Description = Input.Description,
            Format = Input.Format,
            StartDate = Input.StartDate.ToUniversalTime(),
            EndDate = Input.EndDate.ToUniversalTime(),
            MaxParticipants = Input.MaxParticipants,
            MatchDeadlineDays = Input.MatchDeadlineDays,
            PointsForWin = Input.PointsForWin,
            PointsForDraw = Input.PointsForDraw,
            PointsForLoss = Input.PointsForLoss,
            UseHandicaps = Input.UseHandicaps,
            Rules = Input.Rules,
            Prizes = Input.Prizes,
            Status = LeagueStatus.Registration
        };

        _db.LeagueSeasons.Add(league);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"League '{league.Name}' created. Add players and generate the schedule to begin.";
        return RedirectToPage("/League/Manage", new { id = league.Id });
    }

    private async Task LoadSelectListsAsync()
    {
        var clubs = await _db.Clubs.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        ClubSelectList = new SelectList(clubs, "Id", "Name");
    }
}
