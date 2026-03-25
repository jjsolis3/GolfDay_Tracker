using GolfDay.Application.Common.Interfaces;
using GolfDay.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<GolfCourse> GolfCourses => Set<GolfCourse>();
    public DbSet<CourseHole> CourseHoles => Set<CourseHole>();
    public DbSet<ClubMembership> ClubMemberships => Set<ClubMembership>();
    public DbSet<ClubMembershipInvitation> ClubMembershipInvitations => Set<ClubMembershipInvitation>();
    public DbSet<ClubJoinRequest> ClubJoinRequests => Set<ClubJoinRequest>();
    public DbSet<GolfEvent> GolfEvents => Set<GolfEvent>();
    public DbSet<EventParticipant> EventParticipants => Set<EventParticipant>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<HoleScore> HoleScores => Set<HoleScore>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentEntry> TournamentEntries => Set<TournamentEntry>();
    public DbSet<LeagueSeason> LeagueSeasons => Set<LeagueSeason>();
    public DbSet<LeagueEntry> LeagueEntries => Set<LeagueEntry>();
    public DbSet<LeagueMatch> LeagueMatches => Set<LeagueMatch>();
    public DbSet<LeagueStanding> LeagueStandings => Set<LeagueStanding>();
    public DbSet<EventAchievement> EventAchievements => Set<EventAchievement>();
    public DbSet<PlayerSeasonStats> PlayerSeasonStats => Set<PlayerSeasonStats>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set UpdatedAt on modified entities
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        // Normalize all DateTime values to UTC before saving.
        // HTML form inputs produce Kind=Unspecified which PostgreSQL timestamptz rejects.
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Unchanged) continue;
            foreach (var property in entry.Properties)
            {
                if (property.CurrentValue is DateTime dt && dt.Kind != DateTimeKind.Utc)
                    property.CurrentValue = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
