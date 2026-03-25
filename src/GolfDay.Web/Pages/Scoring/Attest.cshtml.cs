using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Pages.Scoring;

[Authorize]
public class AttestModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AttestModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public int RoundId { get; set; }

    public Round? Round { get; set; }
    public List<HoleScore> HoleScores { get; set; } = new();
    public bool AlreadyAttested { get; set; }
    public bool IsRoundOwner { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Round = await _db.Rounds
            .Include(r => r.User)
            .Include(r => r.Event)
            .Include(r => r.Course)
            .FirstOrDefaultAsync(r => r.Id == RoundId);

        if (Round is null) return NotFound();

        IsRoundOwner = Round.UserId == userId;
        AlreadyAttested = Round.IsAttested;

        HoleScores = await _db.HoleScores
            .Where(h => h.RoundId == RoundId)
            .OrderBy(h => h.HoleNumber)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAttestAsync(string? notes)
    {
        var userId = _userManager.GetUserId(User)!;
        var round = await _db.Rounds
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == RoundId);

        if (round is null) return NotFound();

        // Can't attest your own round
        if (round.UserId == userId)
        {
            TempData["ErrorMessage"] = "You cannot attest your own scorecard.";
            return RedirectToPage(new { roundId = RoundId });
        }

        // Must be in same event
        var inSameEvent = await _db.Rounds
            .AnyAsync(r => r.EventId == round.EventId && r.UserId == userId);
        if (!inSameEvent)
        {
            TempData["ErrorMessage"] = "You must have played in the same event to attest this scorecard.";
            return RedirectToPage(new { roundId = RoundId });
        }

        round.IsAttested = true;
        round.AttestedByUserId = userId;
        round.AttestedAt = DateTime.UtcNow;
        round.AttestationNotes = notes?.Trim();
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"You have attested {round.User.FullName}'s scorecard. Thank you!";
        return RedirectToPage("/Events/Details", new { id = round.EventId });
    }

    public async Task<IActionResult> OnPostDisputeAsync(string reason)
    {
        var userId = _userManager.GetUserId(User)!;
        var round = await _db.Rounds
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == RoundId);

        if (round is null) return NotFound();

        round.AttestationNotes = $"[DISPUTED by {userId}]: {reason}";
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Score dispute submitted. The club manager has been notified.";
        return RedirectToPage("/Events/Details", new { id = round.EventId });
    }
}
