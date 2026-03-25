using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Clubs;

[Authorize(Policy = "AdminPolicy")]
public class FinancialModel : PageModel
{
    private readonly IApplicationDbContext _db;
    public FinancialModel(IApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }

    public GolfDay.Domain.Entities.Club? Club { get; set; }
    public List<ClubMembership> Members { get; set; } = new();

    // Summary stats
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
    public int NeverPaidCount { get; set; }
    public List<ClubMembership> OverdueMembers { get; set; } = new();
    public List<ClubMembership> CurrentMembers { get; set; } = new();
    public List<ClubMembership> NeverPaidMembers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Club = await _db.Clubs.FirstOrDefaultAsync(c => c.Id == Id);
        if (Club is null) return NotFound();

        Members = await _db.ClubMemberships
            .Include(m => m.User)
            .Where(m => m.ClubId == Id && m.IsActive)
            .OrderBy(m => m.User.LastName).ThenBy(m => m.User.FirstName)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        CurrentMembers = Members.Where(m => m.DuesPaidThrough.HasValue && m.DuesPaidThrough.Value >= today).ToList();
        OverdueMembers = Members.Where(m => m.DuesPaidThrough.HasValue && m.DuesPaidThrough.Value < today).ToList();
        NeverPaidMembers = Members.Where(m => !m.DuesPaidThrough.HasValue && m.AnnualDueAmount > 0).ToList();

        TotalCollected = Members.Sum(m => m.TotalPaid);
        TotalOutstanding = Members
            .Where(m => m.AnnualDueAmount.HasValue)
            .Sum(m => m.AnnualDueAmount!.Value) - TotalCollected;
        if (TotalOutstanding < 0) TotalOutstanding = 0;

        PaidCount = CurrentMembers.Count;
        OverdueCount = OverdueMembers.Count;
        NeverPaidCount = NeverPaidMembers.Count;

        return Page();
    }

    public async Task<IActionResult> OnPostRecordPaymentAsync(
        int membershipId, decimal amount, string? notes)
    {
        var m = await _db.ClubMemberships.FirstOrDefaultAsync(x => x.Id == membershipId && x.ClubId == Id);
        if (m is null) return NotFound();

        m.LastPaymentDate = DateTime.UtcNow;
        m.LastPaymentAmount = amount;
        m.TotalPaid += amount;
        m.DuesPaidThrough = DateTime.UtcNow.AddYears(1);
        if (!string.IsNullOrWhiteSpace(notes)) m.PaymentNotes = notes.Trim();
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Payment of ${amount:F2} recorded.";
        return RedirectToPage(new { id = Id });
    }
}
