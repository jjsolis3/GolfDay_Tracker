using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Clubs;

[Authorize(Policy = "AdminPolicy")]
public class SponsorsModel : PageModel
{
    private readonly IApplicationDbContext _db;
    public SponsorsModel(IApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public GolfDay.Domain.Entities.Club? Club { get; set; }
    public List<Sponsor> Sponsors { get; set; } = new();
    public List<GolfEvent> RecentEvents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();
        Sponsors = await _db.Sponsors
            .Where(s => s.ClubId == Id)
            .OrderBy(s => s.Tier).ThenBy(s => s.Name)
            .ToListAsync();
        RecentEvents = await _db.GolfEvents
            .Where(e => e.ClubId == Id)
            .OrderByDescending(e => e.EventDate)
            .Take(10).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync(
        string name, string? logoUrl, string? website, string? description,
        int tier, string? contactName, string? contactEmail, int? sponsorshipYear)
    {
        _db.Sponsors.Add(new Sponsor
        {
            ClubId = Id, Name = name.Trim(), LogoUrl = logoUrl?.Trim(),
            Website = website?.Trim(), Description = description?.Trim(),
            ContactName = contactName?.Trim(), ContactEmail = contactEmail?.Trim(),
            Tier = (SponsorTier)tier, IsActive = true,
            SponsorshipYear = sponsorshipYear
        });
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{name} added as a sponsor.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int sponsorId)
    {
        var s = await _db.Sponsors.FirstOrDefaultAsync(x => x.Id == sponsorId && x.ClubId == Id);
        if (s is null) return NotFound();
        s.IsActive = !s.IsActive;
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = s.IsActive ? "Sponsor activated." : "Sponsor deactivated.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int sponsorId)
    {
        var s = await _db.Sponsors.FirstOrDefaultAsync(x => x.Id == sponsorId && x.ClubId == Id);
        if (s is not null) { _db.Sponsors.Remove(s); await _db.SaveChangesAsync(); }
        TempData["SuccessMessage"] = "Sponsor removed.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostAssignToEventAsync(int sponsorId, int eventId)
    {
        var exists = await _db.EventSponsors.AnyAsync(es => es.SponsorId == sponsorId && es.EventId == eventId);
        if (!exists)
        {
            _db.EventSponsors.Add(new EventSponsor { SponsorId = sponsorId, EventId = eventId });
            await _db.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "Sponsor assigned to event.";
        return RedirectToPage(new { id = Id });
    }
}
