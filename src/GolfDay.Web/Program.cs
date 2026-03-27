using GolfDay.Application.Common.Interfaces;
using GolfDay.Infrastructure;
using GolfDay.Infrastructure.Data;
using GolfDay.Web.Hubs;
using GolfDay.Web.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog structured logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/golfday-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Infrastructure (EF Core, Identity, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// HTTP Context accessor (needed for current user service)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IClubAccessService, ClubAccessService>();

// Golf Course API integration
builder.Services.Configure<GolfCourseApiOptions>(
    builder.Configuration.GetSection(GolfCourseApiOptions.Section));
builder.Services.AddHttpClient<IGolfCourseApiService, GolfCourseApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Razor Pages + Controllers (for API endpoints)
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminPolicy");
    options.Conventions.AuthorizePage("/Account/Profile");
    options.Conventions.AuthorizePage("/Club/Dashboard");
    options.Conventions.AuthorizePage("/Scoring/Live");
})
;

builder.Services.AddControllers();
builder.Services.AddSignalR();

// Authorization policies
// Role hierarchy:
//   Admin     – DevAdmin / system-wide access (courses, users, all clubs)
//   ClubAdmin – (formerly ClubManager) manages their own club, events, leagues
//   Member    – authenticated club member
builder.Services.AddAuthorization(options =>
{
    // Whole admin area: Admins + ClubAdmins (ClubAdmin pages further scope by club ownership)
    options.AddPolicy("AdminPolicy",      policy => policy.RequireRole("Admin", "ClubAdmin", "ClubManager"));
    // Course / global management: DevAdmin only
    options.AddPolicy("DevAdminPolicy",   policy => policy.RequireRole("Admin"));
    // Club-specific management: ClubAdmins + Admins
    options.AddPolicy("ClubAdminPolicy",  policy => policy.RequireRole("Admin", "ClubAdmin", "ClubManager"));
    options.AddPolicy("MemberPolicy",     policy => policy.RequireAuthenticatedUser());
});

// Cookie auth settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Response compression
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

// Memory cache for leaderboards/standings
builder.Services.AddMemoryCache();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "Database migration/seeding failed.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseSerilogRequestLogging();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Club membership gate — authenticated non-members are redirected to /Club/Join
// when they try to access club-specific features.
app.Use(async (context, next) =>
{
    var user = context.User;
    if (user.Identity?.IsAuthenticated == true
        && !user.IsInRole("Admin")
        && !user.IsInRole("ClubManager"))
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var lower = path.ToLowerInvariant();

        var isGated =
            lower.StartsWith("/events") ||
            lower.StartsWith("/league") ||
            lower.StartsWith("/tournaments") ||
            lower.StartsWith("/club/leaderboard") ||
            lower.StartsWith("/teetime");

        if (isGated)
        {
            var svc    = context.RequestServices.GetRequiredService<IClubAccessService>();
            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId != null && !await svc.HasActiveMembershipAsync(userId))
            {
                context.Response.Redirect("/Club/Join");
                return;
            }
        }
    }
    await next();
});

app.MapRazorPages();
app.MapControllers();
app.MapHub<ScoringHub>("/hubs/scoring");
app.MapHealthChecks("/health");

app.Run();
