using GolfDay.Domain.Entities;
using GolfDay.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GolfDay.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var db = services.GetRequiredService<ApplicationDbContext>();

        // Seed Roles
        string[] roles = { "Admin", "ClubManager", "Member" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }

        // Seed demo admin user (dev only - should be removed or guarded in prod)
        var adminEmail = "admin@golfdaytracker.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(admin, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Seeded admin user: {Email}", adminEmail);

                // Seed a demo club
                if (!await db.Clubs.AnyAsync())
                {
                    var club = new Club
                    {
                        Name = "Pine Valley Golf Club",
                        Slug = "pine-valley",
                        Description = "A premier golf club for enthusiasts of all skill levels.",
                        City = "Cherry Hill",
                        State = "NJ",
                        Country = "USA",
                        ContactEmail = "info@pinevalley.com",
                        IsActive = true,
                        IsPublic = true
                    };
                    db.Clubs.Add(club);
                    await db.SaveChangesAsync();

                    // Add admin as club manager
                    db.ClubMemberships.Add(new ClubMembership
                    {
                        ClubId = club.Id,
                        UserId = admin.Id,
                        Role = ClubRole.Admin,
                        IsActive = true
                    });

                    // Seed a demo course
                    var course = new GolfCourse
                    {
                        ClubId = club.Id,
                        Name = "Pine Valley Main Course",
                        City = "Cherry Hill",
                        State = "NJ",
                        NumberOfHoles = 18,
                        ParTotal = 72,
                        CourseRating = 74.3,
                        SlopeRating = 155.0,
                        IsActive = true,
                        IsHomeClubCourse = true
                    };
                    db.GolfCourses.Add(course);
                    await db.SaveChangesAsync();

                    // Seed 18 holes
                    var holePars = new[] { 4, 4, 3, 4, 5, 4, 4, 3, 4, 4, 4, 3, 4, 5, 4, 3, 4, 5 };
                    var hcpIndex = new[] { 7, 15, 11, 1, 5, 9, 13, 17, 3, 6, 14, 10, 2, 8, 16, 18, 4, 12 };
                    var yards = new[] { 427, 367, 185, 461, 540, 391, 356, 145, 432, 443, 392, 195, 445, 567, 401, 155, 375, 571 };

                    for (int i = 0; i < 18; i++)
                    {
                        db.CourseHoles.Add(new CourseHole
                        {
                            CourseId = course.Id,
                            HoleNumber = i + 1,
                            Par = holePars[i],
                            HandicapIndex = hcpIndex[i],
                            YardsBack = yards[i],
                            YardsMid = (int)(yards[i] * 0.92),
                            YardsFront = (int)(yards[i] * 0.85)
                        });
                    }

                    await db.SaveChangesAsync();
                    logger.LogInformation("Seeded demo club and course.");
                }
            }
        }

        await userManager.AddToRoleAsync(
            (await userManager.FindByEmailAsync(adminEmail))!,
            "ClubManager");
    }
}
