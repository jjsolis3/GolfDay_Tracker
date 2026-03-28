using GolfDay.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace GolfDay.Web.Areas.Admin.Pages.Courses;

[Authorize(Policy = "AdminPolicy")]
public class ApiSearchModel : PageModel
{
    private readonly IGolfCourseApiService _api;

    public ApiSearchModel(IGolfCourseApiService api) => _api = api;

    // ── Input ────────────────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public string? Query { get; set; }
    [BindProperty(SupportsGet = true)] public string? State { get; set; }

    // ── Output ───────────────────────────────────────────────────────────────
    public List<CourseApiResult> Results    { get; private set; } = new();
    public string?               ApiError   { get; private set; }
    public bool                  HasSearched { get; private set; }
    public bool                  IsConfigured => _api.IsConfigured;

    /// States available in the filter dropdown.
    /// If AllowedStates is configured only those appear; otherwise all US states.
    public List<(string Code, string Name)> StateOptions { get; private set; } = new();

    public async Task OnGetAsync()
    {
        BuildStateOptions();

        if (string.IsNullOrWhiteSpace(Query)) return;

        HasSearched = true;
        (Results, ApiError) = await _api.SearchAsync(Query.Trim(), State);
    }

    /// Called when the admin clicks "Import" — teeIndices is a comma-separated list
    /// of selected tee indices (e.g. "0,2,3").  If empty all tees are imported.
    public IActionResult OnPostPreImport(string courseJson, string? teeIndices)
    {
        try
        {
            var result = JsonSerializer.Deserialize<CourseApiResult>(courseJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null) return BadRequest();

            // Collect the selected tees
            List<CourseTeeInfo> selected;
            if (!string.IsNullOrWhiteSpace(teeIndices))
            {
                var indices = teeIndices.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s.Trim(), out var n) ? n : -1)
                    .Where(n => n >= 0 && n < result.Tees.Count)
                    .Distinct()
                    .ToList();

                selected = indices.Count > 0
                    ? indices.Select(i => result.Tees[i]).ToList()
                    : result.Tees;
            }
            else
            {
                selected = result.Tees;
            }

            // Primary tee: first non-female tee in the selection (for backward-compat fields)
            var primary = selected.FirstOrDefault() ?? result.Tees.FirstOrDefault();

            var import = new CourseApiImportData
            {
                ApiId        = result.ApiId,
                Name         = result.Name,
                Address      = result.Address,
                City         = result.City,
                State        = result.State,
                Zip          = result.Zip,
                Country      = result.Country ?? "US",
                Phone        = result.Phone,
                Website      = result.Website,
                Holes        = result.Holes,
                Par          = primary?.Par          ?? 72,
                CourseRating = primary?.CourseRating,
                SlopeRating  = primary?.SlopeRating,
                TeeUsed      = primary?.TeeName,
                SelectedTees = selected,
            };

            TempData["ApiCourse"] = JsonSerializer.Serialize(import);
            return RedirectToPage("Edit");
        }
        catch
        {
            return RedirectToPage("ApiSearch");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private void BuildStateOptions()
    {
        var all = AllUsStates();

        // Normalise AllowedStates: accept both codes ("CA") and full names ("California")
        var allowed = _api.AllowedStates
            .Select(s => s.Trim().ToUpperInvariant())
            .ToHashSet();

        StateOptions = allowed.Count > 0
            ? all.Where(s => allowed.Contains(s.Code.ToUpperInvariant()) ||
                             allowed.Contains(s.Name.ToUpperInvariant())).ToList()
            : all;
    }

    private static List<(string Code, string Name)> AllUsStates() => new()
    {
        ("AL","Alabama"),("AK","Alaska"),("AZ","Arizona"),("AR","Arkansas"),
        ("CA","California"),("CO","Colorado"),("CT","Connecticut"),("DE","Delaware"),
        ("FL","Florida"),("GA","Georgia"),("HI","Hawaii"),("ID","Idaho"),
        ("IL","Illinois"),("IN","Indiana"),("IA","Iowa"),("KS","Kansas"),
        ("KY","Kentucky"),("LA","Louisiana"),("ME","Maine"),("MD","Maryland"),
        ("MA","Massachusetts"),("MI","Michigan"),("MN","Minnesota"),("MS","Mississippi"),
        ("MO","Missouri"),("MT","Montana"),("NE","Nebraska"),("NV","Nevada"),
        ("NH","New Hampshire"),("NJ","New Jersey"),("NM","New Mexico"),("NY","New York"),
        ("NC","North Carolina"),("ND","North Dakota"),("OH","Ohio"),("OK","Oklahoma"),
        ("OR","Oregon"),("PA","Pennsylvania"),("RI","Rhode Island"),("SC","South Carolina"),
        ("SD","South Dakota"),("TN","Tennessee"),("TX","Texas"),("UT","Utah"),
        ("VT","Vermont"),("VA","Virginia"),("WA","Washington"),("WV","West Virginia"),
        ("WI","Wisconsin"),("WY","Wyoming"),
        // Canadian provinces
        ("AB","Alberta"),("BC","British Columbia"),("MB","Manitoba"),
        ("NB","New Brunswick"),("NL","Newfoundland"),("NS","Nova Scotia"),
        ("ON","Ontario"),("PE","PEI"),("QC","Quebec"),("SK","Saskatchewan"),
    };
}
