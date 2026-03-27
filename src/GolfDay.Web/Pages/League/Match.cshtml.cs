using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GolfDay.Web.Pages.League;

[Authorize]
public class MatchModel : PageModel
{
    private readonly IApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MatchModel(IApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public LeagueMatch Match { get; set; } = null!;
    public ApplicationUser Opponent { get; set; } = null!;
    public ApplicationUser Me { get; set; } = null!;
    public List<Message> Thread { get; set; } = new();
    public bool IsPlayer1 { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;

    [BindProperty]
    public string? MessageBody { get; set; }

    [BindProperty]
    public DateTime? ProposedDate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        CurrentUserId = user.Id;
        Me = user;

        var leagueMatch = await _db.LeagueMatches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .Include(m => m.Course)
            .Include(m => m.LeagueSeason)
                .ThenInclude(ls => ls.Club)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (leagueMatch == null) return NotFound();
        Match = leagueMatch;

        if (Match.Player1Id != user.Id && Match.Player2Id != user.Id)
            return Forbid();

        IsPlayer1 = Match.Player1Id == user.Id;
        Opponent  = IsPlayer1 ? Match.Player2! : Match.Player1!;

        Thread = await _db.Messages
            .Include(m => m.FromUser)
            .Where(m => m.LeagueMatchId == id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        // Mark received unread messages as read
        var unread = Thread.Where(m => m.ToUserId == user.Id && !m.IsRead).ToList();
        if (unread.Any())
        {
            foreach (var msg in unread)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSendMessageAsync(int id)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        var match = await _db.LeagueMatches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match == null) return NotFound();
        if (match.Player1Id != user.Id && match.Player2Id != user.Id) return Forbid();

        if (string.IsNullOrWhiteSpace(MessageBody))
        {
            TempData["ErrorMessage"] = "Message cannot be empty.";
            return RedirectToPage(new { id });
        }

        var toUserId = match.Player1Id == user.Id ? match.Player2Id : match.Player1Id;

        _db.Messages.Add(new Message
        {
            FromUserId    = user.Id,
            ToUserId      = toUserId,
            LeagueMatchId = id,
            Body          = MessageBody.Trim(),
            IsRead        = false
        });

        var preview = MessageBody.Length > 80 ? MessageBody[..80] + "…" : MessageBody;
        _db.Notifications.Add(new Notification
        {
            UserId    = toUserId,
            Type      = NotificationType.LeagueMatchMessage,
            Title     = $"Message from {user.FullName}",
            Message   = preview,
            ActionUrl = $"/League/Match/{id}",
            IsRead    = false
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Message sent.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostProposeDateAsync(int id)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        var match = await _db.LeagueMatches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match == null) return NotFound();
        if (match.Player1Id != user.Id && match.Player2Id != user.Id) return Forbid();

        if (!ProposedDate.HasValue || ProposedDate.Value.Date < DateTime.Today)
        {
            TempData["ErrorMessage"] = "Please select a valid date (today or later).";
            return RedirectToPage(new { id });
        }

        match.ProposedDate = DateTime.SpecifyKind(ProposedDate.Value.Date, DateTimeKind.Utc);
        match.ProposedBy   = user.Id;

        var toUserId  = match.Player1Id == user.Id ? match.Player2Id : match.Player1Id;
        var dateLabel = ProposedDate.Value.ToString("dddd, MMMM d, yyyy");

        _db.Notifications.Add(new Notification
        {
            UserId    = toUserId,
            Type      = NotificationType.LeagueMatchDateProposed,
            Title     = "Match Date Proposed",
            Message   = $"{user.FullName} proposed {dateLabel} for your Round {match.RoundNumber} match.",
            ActionUrl = $"/League/Match/{id}",
            IsRead    = false
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Proposed date: {dateLabel}.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostConfirmDateAsync(int id)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null) return RedirectToPage("/Account/Login");

        var match = await _db.LeagueMatches
            .Include(m => m.Player1)
            .Include(m => m.Player2)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match == null) return NotFound();
        if (match.Player1Id != user.Id && match.Player2Id != user.Id) return Forbid();

        if (!match.ProposedDate.HasValue)
        {
            TempData["ErrorMessage"] = "No date has been proposed yet.";
            return RedirectToPage(new { id });
        }

        // Don't let the proposer confirm their own suggestion
        if (match.ProposedBy == user.Id)
        {
            TempData["ErrorMessage"] = "You proposed this date — wait for your opponent to confirm.";
            return RedirectToPage(new { id });
        }

        var toUserId  = match.Player1Id == user.Id ? match.Player2Id : match.Player1Id;
        var dateLabel = match.ProposedDate.Value.ToString("dddd, MMMM d, yyyy");

        _db.Notifications.Add(new Notification
        {
            UserId    = toUserId,
            Type      = NotificationType.LeagueMatchScheduled,
            Title     = "Match Date Confirmed!",
            Message   = $"{user.FullName} confirmed {dateLabel} for your Round {match.RoundNumber} match.",
            ActionUrl = $"/League/Match/{id}",
            IsRead    = false
        });

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Match confirmed for {dateLabel}.";
        return RedirectToPage(new { id });
    }
}
