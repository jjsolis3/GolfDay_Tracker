using GolfDay.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GolfDay.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Club> Clubs { get; }
    DbSet<GolfCourse> GolfCourses { get; }
    DbSet<CourseHole> CourseHoles { get; }
    DbSet<ClubMembership> ClubMemberships { get; }
    DbSet<ClubMembershipInvitation> ClubMembershipInvitations { get; }
    DbSet<ClubJoinRequest> ClubJoinRequests { get; }
    DbSet<GolfEvent> GolfEvents { get; }
    DbSet<EventParticipant> EventParticipants { get; }
    DbSet<Round> Rounds { get; }
    DbSet<HoleScore> HoleScores { get; }
    DbSet<Tournament> Tournaments { get; }
    DbSet<TournamentEntry> TournamentEntries { get; }
    DbSet<LeagueSeason> LeagueSeasons { get; }
    DbSet<LeagueEntry> LeagueEntries { get; }
    DbSet<LeagueMatch> LeagueMatches { get; }
    DbSet<LeagueStanding> LeagueStandings { get; }
    DbSet<EventAchievement> EventAchievements { get; }
    DbSet<PlayerSeasonStats> PlayerSeasonStats { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<TeeTimeSlot> TeeTimeSlots { get; }
    DbSet<TeeTimeBooking> TeeTimeBookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
