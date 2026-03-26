using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Web.Areas.Admin.Pages.Courses;

[Authorize(Policy = "AdminPolicy")]
public class ImportModel : PageModel
{
    private readonly IApplicationDbContext _db;

    public ImportModel(IApplicationDbContext db) => _db = db;

    [BindProperty] public IFormFile? CsvFile    { get; set; }
    [BindProperty] public int?       DefaultClubId { get; set; }
    [BindProperty] public bool       IsPublic   { get; set; } = true;
    [BindProperty] public bool       PreviewOnly { get; set; } = true;

    public SelectList ClubOptions { get; set; } = null!;
    public List<CourseRow> Preview  { get; set; } = new();
    public List<string>    Errors   { get; set; } = new();
    public int ImportedCount        { get; set; }

    public record CourseRow(
        string Name, string? City, string? State,
        int Holes, int Par,
        double? Rating, double? Slope,
        string? Address, string? Phone, string? Website);

    public async Task OnGetAsync()
    {
        ClubOptions = new SelectList(await _db.Clubs.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ClubOptions = new SelectList(await _db.Clubs.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");

        if (CsvFile is null || CsvFile.Length == 0)
        {
            ModelState.AddModelError("CsvFile", "Please select a CSV file.");
            return Page();
        }

        // Parse CSV
        var rows = new List<CourseRow>();
        using var reader = new System.IO.StreamReader(CsvFile.OpenReadStream());

        string? headerLine = await reader.ReadLineAsync();
        if (headerLine is null) { Errors.Add("File is empty."); return Page(); }

        // Map header columns (case-insensitive)
        var headers = headerLine.Split(',')
            .Select((h, i) => (h.Trim().ToLowerInvariant(), i))
            .ToDictionary(x => x.Item1, x => x.i);

        int Col(string name) => headers.TryGetValue(name, out var idx) ? idx : -1;

        int lineNum = 1;
        while (!reader.EndOfStream)
        {
            lineNum++;
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = SplitCsvLine(line);

            string Get(string colName)
            {
                var idx = Col(colName);
                return idx >= 0 && idx < cols.Length ? cols[idx].Trim() : "";
            }

            var name = Get("name");
            if (string.IsNullOrWhiteSpace(name)) { Errors.Add($"Line {lineNum}: Name is required."); continue; }

            if (!int.TryParse(Get("numberofholes") is { Length: > 0 } h ? h : "18", out var holes)) holes = 18;
            if (!int.TryParse(Get("partotal") is { Length: > 0 } p ? p : "72", out var par)) par = 72;

            double? rating = double.TryParse(Get("courserating"), out var r) ? r : null;
            double? slope  = double.TryParse(Get("sloperating"),  out var s) ? s : null;

            rows.Add(new CourseRow(name, Get("city").NullIfEmpty(), Get("state").NullIfEmpty(),
                holes, par, rating, slope,
                Get("address").NullIfEmpty(), Get("phone").NullIfEmpty(), Get("website").NullIfEmpty()));
        }

        Preview = rows;

        if (PreviewOnly || Errors.Any()) return Page();

        // Actually import
        foreach (var row in rows)
        {
            var course = new GolfCourse
            {
                Name          = row.Name,
                ClubId        = DefaultClubId,
                City          = row.City,
                State         = row.State,
                Address       = row.Address,
                Phone         = row.Phone,
                Website       = row.Website,
                NumberOfHoles = row.Holes,
                ParTotal      = row.Par,
                CourseRating  = row.Rating,
                SlopeRating   = row.Slope,
                IsPublic      = IsPublic,
                IsActive      = true,
            };
            _db.GolfCourses.Add(course);
            ImportedCount++;
        }

        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Successfully imported {ImportedCount} courses.";
        return RedirectToPage("Index");
    }

    private static string[] SplitCsvLine(string line)
    {
        // Handles quoted fields containing commas
        var result = new List<string>();
        var field  = new System.Text.StringBuilder();
        bool inQuotes = false;
        foreach (char c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(field.ToString()); field.Clear(); continue; }
            field.Append(c);
        }
        result.Add(field.ToString());
        return result.ToArray();
    }
}

file static class StringExtensions
{
    public static string? NullIfEmpty(this string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
}
