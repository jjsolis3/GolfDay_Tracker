using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using GolfDay.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GolfDay.Web.Areas.Admin.Pages.Courses;

[Authorize(Policy = "AdminPolicy")]
public class EditModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public EditModel(IApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int? Id { get; set; }

    [BindProperty] public InputModel Input { get; set; } = new();

    public SelectList ClubOptions { get; set; } = null!;
    public bool IsNew => Id is null;

    /// Set when the form was pre-filled from the Golf Course API search.
    public CourseApiImportData? ApiSource { get; private set; }

    public class InputModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int? ClubId { get; set; }

        [MaxLength(2000)] public string? Description { get; set; }
        [MaxLength(300)]  public string? Address      { get; set; }
        [MaxLength(100)]  public string? City         { get; set; }
        [MaxLength(50)]   public string? State        { get; set; }
        [MaxLength(20)]   public string? ZipCode      { get; set; }
        [MaxLength(50)]   public string? Country      { get; set; } = "USA";
        [MaxLength(20)]   public string? Phone        { get; set; }
        [MaxLength(300)]  public string? Website      { get; set; }

        [Range(9, 36, ErrorMessage = "Holes must be 9 or 18 (or multiples up to 36).")]
        public int NumberOfHoles { get; set; } = 18;

        [Range(27, 144)] public int ParTotal { get; set; } = 72;

        [Range(55.0, 80.0)] public double? CourseRating { get; set; }
        [Range(55.0, 155.0)] public double? SlopeRating { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadClubsAsync();

        // Pre-populate from API import if TempData carries an ApiCourse payload
        if (Id is null && TempData.TryGetValue("ApiCourse", out var raw) && raw is string json)
        {
            ApiSource = JsonSerializer.Deserialize<CourseApiImportData>(json);
            if (ApiSource is not null)
            {
                Input = new InputModel
                {
                    Name          = ApiSource.Name,
                    Address       = ApiSource.Address,
                    City          = ApiSource.City,
                    State         = ApiSource.State,
                    ZipCode       = ApiSource.Zip,
                    Country       = ApiSource.Country ?? "USA",
                    Phone         = ApiSource.Phone,
                    Website       = ApiSource.Website,
                    NumberOfHoles = ApiSource.Holes,
                    ParTotal      = ApiSource.Par,
                    CourseRating  = ApiSource.CourseRating,
                    SlopeRating   = ApiSource.SlopeRating,
                    IsActive      = true,
                    IsPublic      = true,
                };
                return Page();
            }
        }

        if (Id is null) return Page();

        var course = await _db.GolfCourses.FindAsync(Id);
        if (course is null) return NotFound();

        Input = new InputModel
        {
            Name          = course.Name,
            ClubId        = course.ClubId,
            Description   = course.Description,
            Address       = course.Address,
            City          = course.City,
            State         = course.State,
            ZipCode       = course.ZipCode,
            Country       = course.Country,
            Phone         = course.Phone,
            Website       = course.Website,
            NumberOfHoles = course.NumberOfHoles,
            ParTotal      = course.ParTotal,
            CourseRating  = course.CourseRating,
            SlopeRating   = course.SlopeRating,
            IsActive      = course.IsActive,
            IsPublic      = course.IsPublic,
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadClubsAsync();
        if (!ModelState.IsValid) return Page();

        GolfCourse course;
        if (Id is null)
        {
            course = new GolfCourse();
            _db.GolfCourses.Add(course);
        }
        else
        {
            course = await _db.GolfCourses.FindAsync(Id) ?? throw new InvalidOperationException();
        }

        course.Name          = Input.Name;
        course.ClubId        = Input.ClubId;
        course.Description   = Input.Description;
        course.Address       = Input.Address;
        course.City          = Input.City;
        course.State         = Input.State;
        course.ZipCode       = Input.ZipCode;
        course.Country       = Input.Country;
        course.Phone         = Input.Phone;
        course.Website       = Input.Website;
        course.NumberOfHoles = Input.NumberOfHoles;
        course.ParTotal      = Input.ParTotal;
        course.CourseRating  = Input.CourseRating;
        course.SlopeRating   = Input.SlopeRating;
        course.IsActive      = Input.IsActive;
        course.IsPublic      = Input.IsPublic;

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Course \"{course.Name}\" {(Id is null ? "created" : "updated")}.";
        return RedirectToPage("Index");
    }

    private async Task LoadClubsAsync()
    {
        var clubs = await _db.Clubs.OrderBy(c => c.Name).ToListAsync();
        ClubOptions = new SelectList(clubs, "Id", "Name");
    }
}
