using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GolfDay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. New columns on ClubMemberships ─────────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "MembershipType",
                table: "ClubMemberships",
                type: "integer",
                nullable: false,
                defaultValue: 2);  // Full

            migrationBuilder.AddColumn<int>(
                name: "MembershipStatus",
                table: "ClubMemberships",
                type: "integer",
                nullable: false,
                defaultValue: 1);  // Active (keeps existing rows active)

            migrationBuilder.AddColumn<bool>(
                name: "CanEnterTournaments",
                table: "ClubMemberships",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanEnterLeagues",
                table: "ClubMemberships",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanBookTeeTime",
                table: "ClubMemberships",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualDueAmount",
                table: "ClubMemberships",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DuesPaidThrough",
                table: "ClubMemberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "ClubMemberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastPaymentAmount",
                table: "ClubMemberships",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNotes",
                table: "ClubMemberships",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPaid",
                table: "ClubMemberships",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "InvitationId",
                table: "ClubMemberships",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "JoinRequestId",
                table: "ClubMemberships",
                type: "integer",
                nullable: true);

            // ── 2. ClubMembershipInvitations ─────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ClubMembershipInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    InvitedByUserId = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IsJoinCode = table.Column<bool>(type: "boolean", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MaxUses = table.Column<int>(type: "integer", nullable: true),
                    UseCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubMembershipInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubMembershipInvitations_AspNetUsers_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubMembershipInvitations_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubMembershipInvitations_Token",
                table: "ClubMembershipInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubMembershipInvitations_ClubId",
                table: "ClubMembershipInvitations",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMembershipInvitations_InvitedByUserId",
                table: "ClubMembershipInvitations",
                column: "InvitedByUserId");

            // ── 3. ClubJoinRequests ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ClubJoinRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ApplicantMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewedByUserId = table.Column<string>(type: "text", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubJoinRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubJoinRequests_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubJoinRequests_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClubJoinRequests_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubJoinRequests_ClubId",
                table: "ClubJoinRequests",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubJoinRequests_UserId",
                table: "ClubJoinRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubJoinRequests_ReviewedByUserId",
                table: "ClubJoinRequests",
                column: "ReviewedByUserId");

            // ── 4. FK indexes on ClubMemberships for Invitation + JoinRequest ─────
            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_InvitationId",
                table: "ClubMemberships",
                column: "InvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubMemberships_JoinRequestId",
                table: "ClubMemberships",
                column: "JoinRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMemberships_ClubMembershipInvitations_InvitationId",
                table: "ClubMemberships",
                column: "InvitationId",
                principalTable: "ClubMembershipInvitations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ClubMemberships_ClubJoinRequests_JoinRequestId",
                table: "ClubMemberships",
                column: "JoinRequestId",
                principalTable: "ClubJoinRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── 5. Notifications ──────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ActionUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            // ── 6. TeeTimeSlots ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TeeTimeSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    SlotDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeeTimeSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeeTimeSlots_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeeTimeSlots_GolfCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "GolfCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeeTimeSlots_ClubId_SlotDate_StartTime",
                table: "TeeTimeSlots",
                columns: new[] { "ClubId", "SlotDate", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TeeTimeSlots_CourseId",
                table: "TeeTimeSlots",
                column: "CourseId");

            // ── 7. TeeTimeBookings ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TeeTimeBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeeTimeSlotId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    NumberOfPlayers = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeeTimeBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeeTimeBookings_TeeTimeSlots_TeeTimeSlotId",
                        column: x => x.TeeTimeSlotId,
                        principalTable: "TeeTimeSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeeTimeBookings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeeTimeBookings_TeeTimeSlotId_UserId",
                table: "TeeTimeBookings",
                columns: new[] { "TeeTimeSlotId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeeTimeBookings_UserId",
                table: "TeeTimeBookings",
                column: "UserId");

            // ── 8. ClubAnnouncements ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "ClubAnnouncements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    Target = table.Column<int>(type: "integer", nullable: false),
                    IsPinned = table.Column<bool>(type: "boolean", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubAnnouncements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubAnnouncements_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubAnnouncements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubAnnouncements_ClubId_IsPublished_IsPinned",
                table: "ClubAnnouncements",
                columns: new[] { "ClubId", "IsPublished", "IsPinned" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubAnnouncements_CreatedByUserId",
                table: "ClubAnnouncements",
                column: "CreatedByUserId");

            // ── 9. Sponsors ───────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SponsorshipYear = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sponsors_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_ClubId_IsActive",
                table: "Sponsors",
                columns: new[] { "ClubId", "IsActive" });

            // ── 10. EventSponsors ─────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "EventSponsors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SponsorId = table.Column<int>(type: "integer", nullable: false),
                    EventId = table.Column<int>(type: "integer", nullable: true),
                    TournamentId = table.Column<int>(type: "integer", nullable: true),
                    LeagueSeasonId = table.Column<int>(type: "integer", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSponsors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSponsors_Sponsors_SponsorId",
                        column: x => x.SponsorId,
                        principalTable: "Sponsors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventSponsors_GolfEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "GolfEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EventSponsors_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EventSponsors_LeagueSeasons_LeagueSeasonId",
                        column: x => x.LeagueSeasonId,
                        principalTable: "LeagueSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_SponsorId",
                table: "EventSponsors",
                column: "SponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_EventId",
                table: "EventSponsors",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_TournamentId",
                table: "EventSponsors",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSponsors_LeagueSeasonId",
                table: "EventSponsors",
                column: "LeagueSeasonId");

            // ── 11. EventWaitlistEntries ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "EventWaitlistEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PromotedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventWaitlistEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventWaitlistEntries_GolfEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "GolfEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventWaitlistEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventWaitlistEntries_EventId_UserId",
                table: "EventWaitlistEntries",
                columns: new[] { "EventId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventWaitlistEntries_EventId_Status_Position",
                table: "EventWaitlistEntries",
                columns: new[] { "EventId", "Status", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_EventWaitlistEntries_UserId",
                table: "EventWaitlistEntries",
                column: "UserId");

            // ── 12. CourseConditionReports ────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CourseConditionReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    ClubId = table.Column<int>(type: "integer", nullable: false),
                    ReportedByUserId = table.Column<string>(type: "text", nullable: false),
                    FairwayCondition = table.Column<int>(type: "integer", nullable: false),
                    GreenSpeed = table.Column<int>(type: "integer", nullable: false),
                    PinPositions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsOfficialReport = table.Column<bool>(type: "boolean", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseConditionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseConditionReports_GolfCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "GolfCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseConditionReports_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseConditionReports_AspNetUsers_ReportedByUserId",
                        column: x => x.ReportedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseConditionReports_CourseId_ReportedAt",
                table: "CourseConditionReports",
                columns: new[] { "CourseId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseConditionReports_ClubId",
                table: "CourseConditionReports",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseConditionReports_ReportedByUserId",
                table: "CourseConditionReports",
                column: "ReportedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClubMemberships_ClubMembershipInvitations_InvitationId",
                table: "ClubMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_ClubMemberships_ClubJoinRequests_JoinRequestId",
                table: "ClubMemberships");

            migrationBuilder.DropTable(name: "CourseConditionReports");
            migrationBuilder.DropTable(name: "EventWaitlistEntries");
            migrationBuilder.DropTable(name: "EventSponsors");
            migrationBuilder.DropTable(name: "Sponsors");
            migrationBuilder.DropTable(name: "ClubAnnouncements");
            migrationBuilder.DropTable(name: "TeeTimeBookings");
            migrationBuilder.DropTable(name: "TeeTimeSlots");
            migrationBuilder.DropTable(name: "Notifications");
            migrationBuilder.DropTable(name: "ClubJoinRequests");
            migrationBuilder.DropTable(name: "ClubMembershipInvitations");

            migrationBuilder.DropIndex(name: "IX_ClubMemberships_InvitationId", table: "ClubMemberships");
            migrationBuilder.DropIndex(name: "IX_ClubMemberships_JoinRequestId", table: "ClubMemberships");

            migrationBuilder.DropColumn(name: "MembershipType", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "MembershipStatus", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "CanEnterTournaments", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "CanEnterLeagues", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "CanBookTeeTime", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "AnnualDueAmount", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "DuesPaidThrough", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "LastPaymentDate", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "LastPaymentAmount", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "PaymentNotes", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "TotalPaid", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "InvitationId", table: "ClubMemberships");
            migrationBuilder.DropColumn(name: "JoinRequestId", table: "ClubMemberships");
        }
    }
}
