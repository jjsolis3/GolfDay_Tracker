-- =============================================================================
-- GolfDay Tracker - Manual Seed Script
-- Run this AFTER the app has started at least once (so EF migrations have run
-- and the admin user + Pine Valley club already exist from DbSeeder).
--
-- Usage (psql):
--   psql -h localhost -U postgres -d golfdaytracker -f seed_data.sql
-- =============================================================================

-- Wrap everything in a transaction so it's all-or-nothing
BEGIN;

-- =============================================================================
-- 1. Grab existing IDs created by DbSeeder
-- =============================================================================
DO $$
DECLARE
    admin_id      TEXT;
    club_id       INT;
    course_id     INT;
BEGIN

SELECT "Id" INTO admin_id FROM "AspNetUsers" WHERE "Email" = 'admin@golfdaytracker.com' LIMIT 1;
SELECT "Id" INTO club_id  FROM "Clubs"       WHERE "Slug" = 'pine-valley'              LIMIT 1;
SELECT "Id" INTO course_id FROM "GolfCourses" WHERE "ClubId" = club_id                 LIMIT 1;

IF admin_id IS NULL THEN
    RAISE EXCEPTION 'Admin user not found – start the app once before running this script.';
END IF;

-- =============================================================================
-- 2. Extra demo members (realistic golfer profiles)
-- =============================================================================
INSERT INTO "AspNetUsers" (
    "Id","UserName","NormalizedUserName","Email","NormalizedEmail",
    "EmailConfirmed","PasswordHash","SecurityStamp","ConcurrencyStamp",
    "PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnabled","AccessFailedCount",
    "FirstName","LastName","HandicapIndex","IsActive","CreatedAt"
) VALUES
-- Password for all demo members: Member@123456
('u-001','rory@pinevalley.com','RORY@PINEVALLEY.COM','rory@pinevalley.com','RORY@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder1==','STAMP001','CONC001',
 FALSE,FALSE,TRUE,0,'Rory','McAlroy',4.2,TRUE,NOW()),

('u-002','tiger@pinevalley.com','TIGER@PINEVALLEY.COM','tiger@pinevalley.com','TIGER@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder2==','STAMP002','CONC002',
 FALSE,FALSE,TRUE,0,'Tiger','Woods',2.1,TRUE,NOW()),

('u-003','phil@pinevalley.com','PHIL@PINEVALLEY.COM','phil@pinevalley.com','PHIL@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder3==','STAMP003','CONC003',
 FALSE,FALSE,TRUE,0,'Phil','Mickelson',6.8,TRUE,NOW()),

('u-004','brooke@pinevalley.com','BROOKE@PINEVALLEY.COM','brooke@pinevalley.com','BROOKE@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder4==','STAMP004','CONC004',
 FALSE,FALSE,TRUE,0,'Brooke','Henderson',3.5,TRUE,NOW()),

('u-005','nelly@pinevalley.com','NELLY@PINEVALLEY.COM','nelly@pinevalley.com','NELLY@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder5==','STAMP005','CONC005',
 FALSE,FALSE,TRUE,0,'Nelly','Korda',1.8,TRUE,NOW()),

('u-006','collin@pinevalley.com','COLLIN@PINEVALLEY.COM','collin@pinevalley.com','COLLIN@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder6==','STAMP006','CONC006',
 FALSE,FALSE,TRUE,0,'Collin','Morikawa',3.0,TRUE,NOW()),

('u-007','xander@pinevalley.com','XANDER@PINEVALLEY.COM','xander@pinevalley.com','XANDER@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder7==','STAMP007','CONC007',
 FALSE,FALSE,TRUE,0,'Xander','Schauffele',2.5,TRUE,NOW()),

('u-008','sam@pinevalley.com','SAM@PINEVALLEY.COM','sam@pinevalley.com','SAM@PINEVALLEY.COM',
 TRUE,'AQAAAAIAAYagAAAAELnxPlaceholder8==','STAMP008','CONC008',
 FALSE,FALSE,TRUE,0,'Sam','Burns',8.4,TRUE,NOW())

ON CONFLICT ("NormalizedEmail") DO NOTHING;

-- Assign Member role to all demo users
INSERT INTO "AspNetUserRoles" ("UserId","RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u
CROSS JOIN "AspNetRoles" r
WHERE u."Id" IN ('u-001','u-002','u-003','u-004','u-005','u-006','u-007','u-008')
  AND r."Name" = 'Member'
ON CONFLICT DO NOTHING;

-- =============================================================================
-- 3. Club memberships for demo users
-- =============================================================================
INSERT INTO "ClubMemberships" (
    "ClubId","UserId","Role","MembershipType","MembershipStatus",
    "IsActive","JoinedAt","CanEnterTournaments","CanEnterLeagues","CanBookTeeTime",
    "TotalPaid","CreatedAt"
) VALUES
(club_id,'u-001',0,2,2,TRUE,NOW()-INTERVAL '6 months',TRUE,TRUE,TRUE,1500,NOW()),
(club_id,'u-002',0,2,2,TRUE,NOW()-INTERVAL '2 years', TRUE,TRUE,TRUE,3000,NOW()),
(club_id,'u-003',0,2,2,TRUE,NOW()-INTERVAL '1 year',  TRUE,TRUE,TRUE,1500,NOW()),
(club_id,'u-004',0,2,2,TRUE,NOW()-INTERVAL '8 months',TRUE,TRUE,TRUE,1500,NOW()),
(club_id,'u-005',0,2,2,TRUE,NOW()-INTERVAL '3 months',TRUE,TRUE,TRUE,750, NOW()),
(club_id,'u-006',0,2,2,TRUE,NOW()-INTERVAL '1 year',  TRUE,TRUE,TRUE,1500,NOW()),
(club_id,'u-007',0,2,2,TRUE,NOW()-INTERVAL '5 months',TRUE,TRUE,TRUE,750, NOW()),
(club_id,'u-008',0,2,2,TRUE,NOW()-INTERVAL '1 year',  TRUE,TRUE,TRUE,1500,NOW())
ON CONFLICT DO NOTHING;

-- =============================================================================
-- 4. Tournament: Spring Invitational (completed)
-- =============================================================================
INSERT INTO "Tournaments" (
    "ClubId","Name","Description","Format","Status","StartDate","EndDate",
    "NumberOfRounds","MaxParticipants","UseHandicaps","IsPublic","CreatedAt"
) VALUES (
    club_id,
    'Spring Invitational 2026',
    'Annual Pine Valley Spring tournament. Net stroke play over 18 holes.',
    0, -- StrokePlay
    3, -- Completed
    NOW()-INTERVAL '30 days',
    NOW()-INTERVAL '29 days',
    1, 32, TRUE, TRUE, NOW()-INTERVAL '60 days'
) RETURNING "Id" INTO STRICT course_id; -- reuse variable for tournament_id

-- tournament entries
INSERT INTO "TournamentEntries" (
    "TournamentId","UserId","Status","HandicapAtEntry","FinalPosition","RegisteredAt","CreatedAt"
) VALUES
(course_id,'u-001',3,4.2,1, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-007',3,2.5,2, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-002',3,2.1,3, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-006',3,3.0,4, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-003',3,6.8,5, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-004',3,3.5,6, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-005',3,1.8,7, NOW()-INTERVAL '45 days',NOW()),
(course_id,'u-008',3,8.4,8, NOW()-INTERVAL '45 days',NOW());

-- =============================================================================
-- 5. GolfEvent for the tournament round
-- =============================================================================
INSERT INTO "GolfEvents" (
    "ClubId","CourseId","TournamentId","Name","EventType","Format",
    "EventDate","Status","MaxParticipants","UseHandicaps","IsPublic","CreatedAt"
) VALUES (
    club_id,
    (SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = club_id LIMIT 1),
    course_id,
    'Spring Invitational – Round 1',
    2, -- Tournament
    'Net Stroke Play',
    NOW()-INTERVAL '30 days',
    3, -- Completed
    32, TRUE, TRUE, NOW()-INTERVAL '31 days'
) RETURNING "Id" INTO STRICT club_id; -- reuse as event_id

-- Register participants
INSERT INTO "EventParticipants" ("EventId","UserId","RegisteredAt","CheckedIn","CreatedAt")
SELECT club_id, u, NOW()-INTERVAL '30 days', TRUE, NOW()
FROM unnest(ARRAY['u-001','u-002','u-003','u-004','u-005','u-006','u-007','u-008']::TEXT[]) AS u
ON CONFLICT DO NOTHING;

-- Rounds (completed, with realistic net scores)
INSERT INTO "Rounds" (
    "EventId","CourseId","UserId","RoundNumber","Status",
    "GrossScore","NetScore","HandicapUsed","TotalPutts","FairwaysHit","TotalFairways",
    "GreensInRegulation","TotalGreens","Birdies","Pars","Bogeys","DoubleBogeys",
    "IsAttested","StartedAt","CompletedAt","CreatedAt"
) VALUES
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-001',1,3,76,72,4.2,30,9,14,10,18,2,9,6,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-007',1,3,74,72,2.5,29,10,14,11,18,3,10,4,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-002',1,3,74,72,2.1,28,11,14,12,18,4,9,4,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-006',1,3,75,72,3.0,31,9,14,10,18,2,10,5,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-003',1,3,79,72,6.8,33,8,14, 9,18,1,8,8,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-004',1,3,76,73,3.5,30,9,14,10,18,2,9,6,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-005',1,3,73,71,1.8,27,12,14,13,18,5,10,2,1,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW()),
(club_id,(SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "ClubId" FROM "GolfEvents" WHERE "Id" = club_id) LIMIT 1),'u-008',1,3,80,72,8.4,34,7,14, 8,18,1,7,8,2,TRUE,NOW()-INTERVAL '30 days',NOW()-INTERVAL '29 days 22 hours',NOW());

-- =============================================================================
-- 6. League Season: Summer Match Play League
-- =============================================================================
INSERT INTO "LeagueSeasons" (
    "ClubId","Name","Format","Status","StartDate","EndDate",
    "MaxParticipants","PointsForWin","PointsForDraw","PointsForLoss",
    "MatchDeadlineDays","UseHandicaps","CreatedAt"
) VALUES (
    (SELECT "Id" FROM "Clubs" WHERE "Slug" = 'pine-valley' LIMIT 1),
    'Summer Match Play League 2026',
    1, -- MatchPlay
    1, -- Active
    NOW()-INTERVAL '14 days',
    NOW()+INTERVAL '60 days',
    16, 3, 1, 0, 14, TRUE,
    NOW()-INTERVAL '20 days'
) RETURNING "Id" INTO STRICT course_id; -- reuse as season_id

-- League entries
INSERT INTO "LeagueEntries" (
    "LeagueSeasonId","UserId","RegisteredAt","IsActive","CreatedAt"
)
SELECT course_id, u, NOW()-INTERVAL '14 days', TRUE, NOW()
FROM unnest(ARRAY['u-001','u-002','u-003','u-004','u-005','u-006','u-007','u-008']::TEXT[]) AS u
ON CONFLICT DO NOTHING;

-- League standings (initial after round 1)
INSERT INTO "LeagueStandings" (
    "LeagueSeasonId","UserId","Rank","Points","Wins","Losses","Draws","Played","CreatedAt"
) VALUES
(course_id,'u-005',1,3,1,0,0,1,NOW()),
(course_id,'u-002',2,3,1,0,0,1,NOW()),
(course_id,'u-001',3,1,0,0,1,1,NOW()),
(course_id,'u-007',4,1,0,0,1,1,NOW()),
(course_id,'u-006',5,0,0,1,0,1,NOW()),
(course_id,'u-003',6,0,0,1,0,1,NOW()),
(course_id,'u-004',7,0,0,1,0,1,NOW()),
(course_id,'u-008',8,0,0,1,0,1,NOW())
ON CONFLICT DO NOTHING;

-- =============================================================================
-- 7. Upcoming Event: Club Championship qualifier (open registration)
-- =============================================================================
INSERT INTO "GolfEvents" (
    "ClubId","CourseId","Name","EventType","Format",
    "EventDate","Status","MaxParticipants","UseHandicaps","IsPublic","CreatedAt"
) VALUES (
    (SELECT "Id" FROM "Clubs" WHERE "Slug" = 'pine-valley' LIMIT 1),
    (SELECT "Id" FROM "GolfCourses" WHERE "ClubId" = (SELECT "Id" FROM "Clubs" WHERE "Slug" = 'pine-valley' LIMIT 1) LIMIT 1),
    'Club Championship Qualifier',
    0, -- Regular
    'Gross Stroke Play',
    NOW()+INTERVAL '14 days',
    1, -- Open
    24, FALSE, TRUE,
    NOW()
);

-- =============================================================================
-- 8. Club Announcement
-- =============================================================================
INSERT INTO "ClubAnnouncements" (
    "ClubId","Title","Body","CreatedByUserId","Target",
    "IsPinned","IsPublished","PublishedAt","CreatedAt"
) VALUES (
    (SELECT "Id" FROM "Clubs" WHERE "Slug" = 'pine-valley' LIMIT 1),
    'Welcome to the New GolfDay Tracker!',
    'We have launched our new online golf tracking system. You can now register for events, track your scores, and follow the league standings all in one place. Contact the pro shop if you have any questions.',
    (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'admin@golfdaytracker.com' LIMIT 1),
    0, -- AllMembers
    TRUE, TRUE, NOW(), NOW()
),
(
    (SELECT "Id" FROM "Clubs" WHERE "Slug" = 'pine-valley' LIMIT 1),
    'Club Championship Qualifier – Register Now',
    'Registration is open for the Club Championship Qualifier on ' || TO_CHAR(NOW()+INTERVAL '14 days','Month DDth') || '. Spots are limited to 24 players. Sign up through the Events page.',
    (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'admin@golfdaytracker.com' LIMIT 1),
    0, -- AllMembers
    FALSE, TRUE, NOW(), NOW()
);

END $$;

COMMIT;

-- Quick verification
SELECT 'Users' AS entity, COUNT(*) FROM "AspNetUsers"
UNION ALL SELECT 'ClubMemberships', COUNT(*) FROM "ClubMemberships"
UNION ALL SELECT 'Tournaments', COUNT(*) FROM "Tournaments"
UNION ALL SELECT 'GolfEvents', COUNT(*) FROM "GolfEvents"
UNION ALL SELECT 'Rounds', COUNT(*) FROM "Rounds"
UNION ALL SELECT 'LeagueSeasons', COUNT(*) FROM "LeagueSeasons"
UNION ALL SELECT 'LeagueStandings', COUNT(*) FROM "LeagueStandings"
UNION ALL SELECT 'ClubAnnouncements', COUNT(*) FROM "ClubAnnouncements";
