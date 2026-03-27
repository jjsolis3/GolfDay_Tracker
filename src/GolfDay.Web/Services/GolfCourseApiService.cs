using System.Text.Json;
using Microsoft.Extensions.Options;

namespace GolfDay.Web.Services;

// ─── Configuration ───────────────────────────────────────────────────────────

public class GolfCourseApiOptions
{
    public const string Section = "GolfCourseApi";

    /// <summary>API key from thegolfcourseapi.com</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL – override only if the provider changes versions.</summary>
    public string BaseUrl { get; set; } = "https://api.golfcourseapi.com/v1";

    /// <summary>
    /// Restrict searches to these US state abbreviations (e.g. ["TX","NM"]).
    /// Empty = allow all states.
    /// </summary>
    public List<string> AllowedStates { get; set; } = new();

    /// <summary>Default country passed to the API when searching.</summary>
    public string DefaultCountry { get; set; } = "US";
}

// ─── Domain models returned to callers ───────────────────────────────────────

public class CourseApiResult
{
    public string ApiId     { get; set; } = string.Empty;
    public string Name      { get; set; } = string.Empty;
    public string? Address  { get; set; }
    public string? City     { get; set; }
    public string? State    { get; set; }
    public string? Zip      { get; set; }
    public string? Country  { get; set; }
    public string? Phone    { get; set; }
    public string? Website  { get; set; }
    public int     Holes    { get; set; } = 18;
    public List<CourseTeeInfo> Tees { get; set; } = new();
}

public class CourseTeeInfo
{
    public string   TeeName      { get; set; } = string.Empty;
    public int      Par          { get; set; } = 72;
    public double?  CourseRating { get; set; }
    public double?  SlopeRating  { get; set; }

    public string Label => $"{TeeName}  —  Par {Par}" +
        (CourseRating.HasValue ? $"  |  {CourseRating:F1} / {SlopeRating:F0}" : "");
}

// Serialisable snapshot stored in TempData when the user picks a result to import
public class CourseApiImportData
{
    public string  ApiId        { get; set; } = string.Empty;
    public string  Name         { get; set; } = string.Empty;
    public string? Address      { get; set; }
    public string? City         { get; set; }
    public string? State        { get; set; }
    public string? Zip          { get; set; }
    public string? Country      { get; set; }
    public string? Phone        { get; set; }
    public string? Website      { get; set; }
    public int     Holes        { get; set; } = 18;
    public int     Par          { get; set; } = 72;
    public double? CourseRating { get; set; }
    public double? SlopeRating  { get; set; }
    public string? TeeUsed      { get; set; }
}

// ─── Service interface & implementation ──────────────────────────────────────

public interface IGolfCourseApiService
{
    bool         IsConfigured  { get; }
    List<string> AllowedStates { get; }

    Task<(List<CourseApiResult> Courses, string? Error)> SearchAsync(
        string query,
        string? state  = null,
        int    page    = 1,
        int    perPage = 20);
}

public class GolfCourseApiService : IGolfCourseApiService
{
    private readonly HttpClient           _http;
    private readonly GolfCourseApiOptions _opts;
    private readonly ILogger<GolfCourseApiService> _log;

    public bool         IsConfigured  => !string.IsNullOrWhiteSpace(_opts.ApiKey);
    public List<string> AllowedStates => _opts.AllowedStates;

    public GolfCourseApiService(
        HttpClient                      http,
        IOptions<GolfCourseApiOptions>  opts,
        ILogger<GolfCourseApiService>   log)
    {
        _http = http;
        _opts = opts.Value;
        _log  = log;
    }

    public async Task<(List<CourseApiResult> Courses, string? Error)> SearchAsync(
        string query,
        string? state  = null,
        int    page    = 1,
        int    perPage = 20)
    {
        if (!IsConfigured)
            return (new(), "Golf Course API key is not configured. Add it under GolfCourseApi:ApiKey in appsettings.");

        // Build query string
        var qs = new List<string>
        {
            $"search={Uri.EscapeDataString(query)}",
            $"per_page={perPage}",
            $"page={page}"
        };

        if (!string.IsNullOrWhiteSpace(state))
            qs.Add($"state={Uri.EscapeDataString(state.Trim())}");

        var url = $"{_opts.BaseUrl.TrimEnd('/')}/courses?{string.Join('&', qs)}";

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            // Golf Course API uses "Authorization: Key {apiKey}" (per activation email)
            req.Headers.Add("Authorization", $"Key {_opts.ApiKey}");

            using var resp = await _http.SendAsync(req);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _log.LogWarning("Golf Course API returned {Code}: {Body}", (int)resp.StatusCode, body);
                return (new(), $"API returned {(int)resp.StatusCode}: {resp.ReasonPhrase}");
            }

            var json = await resp.Content.ReadAsStringAsync();
            var courses = ParseCourses(json);

            // Apply region filter (extra safety if the API doesn't honour state param).
            // AllowedStates may contain codes ("CA") or full names ("California") — normalise
            // both for comparison against API result states (which are typically codes).
            if (_opts.AllowedStates.Count > 0)
            {
                var allowedCodes = _opts.AllowedStates
                    .Select(s => s.Trim().ToUpperInvariant())
                    .ToHashSet();

                // Also resolve full names → codes using the same list as the search page
                var nameToCode = StateNameToCode();
                foreach (var entry in _opts.AllowedStates)
                {
                    var norm = entry.Trim().ToUpperInvariant();
                    if (nameToCode.TryGetValue(norm, out var code))
                        allowedCodes.Add(code);
                }

                courses = courses
                    .Where(c => c.State is null ||
                                allowedCodes.Contains(c.State.Trim().ToUpperInvariant()))
                    .ToList();
            }

            return (courses, null);
        }
        catch (HttpRequestException ex)
        {
            _log.LogError(ex, "Golf Course API HTTP error");
            return (new(), $"Network error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Golf Course API unexpected error");
            return (new(), $"Unexpected error: {ex.Message}");
        }
    }

    // ── JSON parsing ─────────────────────────────────────────────────────────
    // The Golf Course API (v1) returns:
    //   { "data": { "courses": [...], "total": N, "per_page": 20, "page": 1 } }
    // Some versions may return courses at the root level; we handle both.
    private static List<CourseApiResult> ParseCourses(string json)
    {
        using var doc  = JsonDocument.Parse(json);
        var root       = doc.RootElement;
        var results    = new List<CourseApiResult>();

        // Locate the courses array
        JsonElement coursesEl;
        if (root.TryGetProperty("data", out var data) &&
            data.TryGetProperty("courses", out coursesEl))
        { /* nested under data */ }
        else if (root.TryGetProperty("courses", out coursesEl))
        { /* top-level */ }
        else
            return results; // unexpected shape

        foreach (var c in coursesEl.EnumerateArray())
        {
            var result = new CourseApiResult();

            result.ApiId = Str(c, "id") ?? string.Empty;
            result.Name  = Str(c, "club_name") ?? Str(c, "name") ?? string.Empty;

            if (c.TryGetProperty("location", out var loc))
            {
                result.Address = Str(loc, "address");
                result.City    = Str(loc, "city");
                result.State   = Str(loc, "state");
                result.Zip     = Str(loc, "zip") ?? Str(loc, "postal_code");
                result.Country = Str(loc, "country");
            }

            if (c.TryGetProperty("contact", out var contact))
            {
                result.Phone   = Str(contact, "phone");
                result.Website = Str(contact, "website");
            }

            // Holes
            if (c.TryGetProperty("num_holes",        out var holes)) result.Holes = holes.GetInt32();
            else if (c.TryGetProperty("number_holes", out holes))    result.Holes = holes.GetInt32();

            // Tees
            if (c.TryGetProperty("tees", out var tees))
            {
                foreach (var tee in tees.EnumerateArray())
                {
                    var info = new CourseTeeInfo
                    {
                        TeeName      = Str(tee, "tee_name") ?? Str(tee, "name") ?? "Unknown",
                        CourseRating = Dbl(tee, "course_rating"),
                        SlopeRating  = Dbl(tee, "slope_rating"),
                        Par          = Int(tee, "par") ?? 72,
                    };
                    result.Tees.Add(info);
                }
            }

            if (!string.IsNullOrWhiteSpace(result.Name))
                results.Add(result);
        }

        return results;
    }

    // ── JSON helpers ──────────────────────────────────────────────────────────
    private static string? Str(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static double? Dbl(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) &&
        (v.ValueKind == JsonValueKind.Number)
            ? v.GetDouble()
            : null;

    private static int? Int(JsonElement el, string key) =>
        el.TryGetProperty(key, out var v) &&
        (v.ValueKind == JsonValueKind.Number)
            ? v.GetInt32()
            : null;

    // Maps uppercased full state names → 2-letter codes for region filtering.
    private static Dictionary<string, string> StateNameToCode() => new()
    {
        {"ALABAMA","AL"},{"ALASKA","AK"},{"ARIZONA","AZ"},{"ARKANSAS","AR"},
        {"CALIFORNIA","CA"},{"COLORADO","CO"},{"CONNECTICUT","CT"},{"DELAWARE","DE"},
        {"FLORIDA","FL"},{"GEORGIA","GA"},{"HAWAII","HI"},{"IDAHO","ID"},
        {"ILLINOIS","IL"},{"INDIANA","IN"},{"IOWA","IA"},{"KANSAS","KS"},
        {"KENTUCKY","KY"},{"LOUISIANA","LA"},{"MAINE","ME"},{"MARYLAND","MD"},
        {"MASSACHUSETTS","MA"},{"MICHIGAN","MI"},{"MINNESOTA","MN"},{"MISSISSIPPI","MS"},
        {"MISSOURI","MO"},{"MONTANA","MT"},{"NEBRASKA","NE"},{"NEVADA","NV"},
        {"NEW HAMPSHIRE","NH"},{"NEW JERSEY","NJ"},{"NEW MEXICO","NM"},{"NEW YORK","NY"},
        {"NORTH CAROLINA","NC"},{"NORTH DAKOTA","ND"},{"OHIO","OH"},{"OKLAHOMA","OK"},
        {"OREGON","OR"},{"PENNSYLVANIA","PA"},{"RHODE ISLAND","RI"},{"SOUTH CAROLINA","SC"},
        {"SOUTH DAKOTA","SD"},{"TENNESSEE","TN"},{"TEXAS","TX"},{"UTAH","UT"},
        {"VERMONT","VT"},{"VIRGINIA","VA"},{"WASHINGTON","WA"},{"WEST VIRGINIA","WV"},
        {"WISCONSIN","WI"},{"WYOMING","WY"},
        {"ALBERTA","AB"},{"BRITISH COLUMBIA","BC"},{"MANITOBA","MB"},
        {"NEW BRUNSWICK","NB"},{"NEWFOUNDLAND","NL"},{"NOVA SCOTIA","NS"},
        {"ONTARIO","ON"},{"PEI","PE"},{"QUEBEC","QC"},{"SASKATCHEWAN","SK"},
    };
}
