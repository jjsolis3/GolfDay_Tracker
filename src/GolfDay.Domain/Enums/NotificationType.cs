namespace GolfDay.Domain.Enums;

public enum NotificationType
{
    // Club
    ClubInvitation = 1,
    JoinRequestApproved = 2,
    JoinRequestRejected = 3,

    // Events
    EventCreated = 10,
    EventReminder = 11,
    EventCancelled = 12,
    EventResultPosted = 13,

    // Scoring
    ScoreAttestationRequested = 20,
    ScoreAttested = 21,
    ScoreRejected = 22,

    // Tee Times
    TeeTimeConfirmed = 30,
    TeeTimeCancelled = 31,
    TeeTimeReminder = 32,

    // League / Tournament
    LeagueMatchScheduled = 40,
    TournamentRegistrationOpen = 41,

    // General
    General = 99
}
