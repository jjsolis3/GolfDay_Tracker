-- =============================================================================
-- GolfDay Tracker - Manual Seed Script (Supabase / PostgreSQL)
-- Run AFTER the app has started once so EF migrations have applied.
-- The admin user (admin@golfdaytracker.com) and Pine Valley club must exist.
-- =============================================================================

BEGIN;

-- =============================================================================
-- 1. Demo users  (NormalizedUserName is the unique column in AspNetUsers)
-- =============================================================================
INSERT INTO "AspNetUsers" (
    "Id","UserName","NormalizedUserName","Email","NormalizedEmail",
    "EmailConfirmed","PasswordHash","SecurityStamp","ConcurrencyStamp",
    "PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnabled","AccessFailedCount",
    "FirstName","LastName","HandicapIndex","IsActive","CreatedAt"
)
SELECT * FROM (VALUES
    ('u-001','rory@pinevalley.com',   'RORY@PINEVALLEY.COM',   'rory@pinevalley.com',   'RORY@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP001','CONC001',FALSE,FALSE,TRUE,0,'Rory',   'McAlroy',    4.2,TRUE,NOW()),
    ('u-002','tiger@pinevalley.com',  'TIGER@PINEVALLEY.COM',  'tiger@pinevalley.com',  'TIGER@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP002','CONC002',FALSE,FALSE,TRUE,0,'Tiger',  'Woods',      2.1,TRUE,NOW()),
    ('u-003','phil@pinevalley.com',   'PHIL@PINEVALLEY.COM',   'phil@pinevalley.com',   'PHIL@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP003','CONC003',FALSE,FALSE,TRUE,0,'Phil',   'Mickelson',  6.8,TRUE,NOW()),
    ('u-004','brooke@pinevalley.com', 'BROOKE@PINEVALLEY.COM', 'brooke@pinevalley.com', 'BROOKE@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP004','CONC004',FALSE,FALSE,TRUE,0,'Brooke', 'Henderson',  3.5,TRUE,NOW()),
    ('u-005','nelly@pinevalley.com',  'NELLY@PINEVALLEY.COM',  'nelly@pinevalley.com',  'NELLY@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP005','CONC005',FALSE,FALSE,TRUE,0,'Nelly',  'Korda',      1.8,TRUE,NOW()),
    ('u-006','collin@pinevalley.com', 'COLLIN@PINEVALLEY.COM', 'collin@pinevalley.com', 'COLLIN@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP006','CONC006',FALSE,FALSE,TRUE,0,'Collin', 'Morikawa',   3.0,TRUE,NOW()),
    ('u-007','xander@pinevalley.com', 'XANDER@PINEVALLEY.COM', 'xander@pinevalley.com', 'XANDER@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP007','CONC007',FALSE,FALSE,TRUE,0,'Xander', 'Schauffele', 2.5,TRUE,NOW()),
    ('u-008','sam@pinevalley.com',    'SAM@PINEVALLEY.COM',    'sam@pinevalley.com',    'SAM@PINEVALLEY.COM',
     TRUE,'AQAAAAIAAYagAAAAELnxPlaceholderHash==','STAMP008','CONC008',FALSE,FALSE,TRUE,0,'Sam',    'Burns',      8.4,TRUE,NOW())
) AS v("Id","UserName","NormalizedUserName","Email","NormalizedEmail",
       "EmailConfirmed","PasswordHash","SecurityStamp","ConcurrencyStamp",
       "PhoneNumberConfirmed","TwoFactorEnabled","LockoutEnabled","AccessFailedCount",
       "FirstName","LastName","HandicapIndex","IsActive","CreatedAt")
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = v."NormalizedUserName"
);

-- =============================================================================
-- 2. Assign Member role
-- =============================================================================
INSERT INTO "AspNetUserRoles" ("UserId","RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u
CROSS JOIN "AspNetRoles" r
WHERE u."Id" IN ('u-001','u-002','u-003','u-004','u-005','u-006','u-007','u-008')
  AND r."Name" = 'Member'
ON CONFLICT ("UserId","RoleId") DO NOTHING;

-- =============================================================================
-- 3. Club memberships
-- =============================================================================
INSERT INTO "ClubMemberships" (
    "ClubId","UserId","Role","MembershipType","MembershipStatus",
    "IsActive","JoinedAt","CanEnterTournaments","CanEnterLeagues","CanBookTeeTime",
    "TotalPaid","CreatedAt"
)
SELECT
    c."Id",
    u."UserId",
    0,   -- ClubRole.Member
    2,   -- MembershipType.Full
    1,   -- MembershipStatus.Active
    TRUE,
    NOW() - (u."MonthsAgo" || ' months')::INTERVAL,
    TRUE, TRUE, TRUE,
    u."Paid",
    NOW()
FROM "Clubs" c
CROSS JOIN (VALUES
    ('u-001',6, 1500::NUMERIC),
    ('u-002',24,3000::NUMERIC),
    ('u-003',12,1500::NUMERIC),
    ('u-004',8, 1500::NUMERIC),
    ('u-005',3, 750::NUMERIC),
    ('u-006',12,1500::NUMERIC),
    ('u-007',5, 750::NUMERIC),
    ('u-008',12,1500::NUMERIC)
) AS u("UserId","MonthsAgo","Paid")
WHERE c."Slug" = 'pine-valley'
  AND NOT EXISTS (
      SELECT 1 FROM "ClubMemberships" cm
      WHERE cm."ClubId" = c."Id" AND cm."UserId" = u."UserId"
  );

-- =============================================================================
-- 4. Tournament: Spring Invitational 2026 (completed)
-- =============================================================================
INSERT INTO "Tournaments" (
    "ClubId","Name","Description","Format","Status",
    "StartDate","EndDate","NumberOfRounds","MaxParticipants",
    "UseHandicaps","IsPublic","CreatedAt"
)
SELECT
    c."Id",
    'Spring Invitational 2026',
    'Annual Pine Valley Spring tournament. Net stroke play over 18 holes.',
    0,   -- StrokePlay
    3,   -- Completed
    NOW()-INTERVAL '30 days',
    NOW()-INTERVAL '29 days',
    1, 32, TRUE, TRUE,
    NOW()-INTERVAL '60 days'
FROM "Clubs" c
WHERE c."Slug" = 'pine-valley'
  AND NOT EXISTS (
      SELECT 1 FROM "Tournaments" WHERE "Name" = 'Spring Invitational 2026'
  );

-- =============================================================================
-- 5. Tournament entries
-- =============================================================================
INSERT INTO "TournamentEntries" (
    "TournamentId","UserId","Status","HandicapAtEntry","FinalPosition","RegisteredAt","CreatedAt"
)
SELECT
    t."Id",
    e."UserId",
    3,   -- Completed
    e."Hcp",
    e."Pos",
    NOW()-INTERVAL '45 days',
    NOW()
FROM "Tournaments" t
CROSS JOIN (VALUES
    ('u-001',4.2::DOUBLE PRECISION,1),
    ('u-007',2.5::DOUBLE PRECISION,2),
    ('u-002',2.1::DOUBLE PRECISION,3),
    ('u-006',3.0::DOUBLE PRECISION,4),
    ('u-003',6.8::DOUBLE PRECISION,5),
    ('u-004',3.5::DOUBLE PRECISION,6),
    ('u-005',1.8::DOUBLE PRECISION,7),
    ('u-008',8.4::DOUBLE PRECISION,8)
) AS e("UserId","Hcp","Pos")
WHERE t."Name" = 'Spring Invitational 2026'
  AND NOT EXISTS (
      SELECT 1 FROM "TournamentEntries" te
      WHERE te."TournamentId" = t."Id" AND te."UserId" = e."UserId"
  );

-- =============================================================================
-- 6. GolfEvent for tournament round
-- =============================================================================
INSERT INTO "GolfEvents" (
    "ClubId","CourseId","TournamentId","Name","EventType","Format",
    "EventDate","Status","MaxParticipants","UseHandicaps","IsPublic","CreatedAt"
)
SELECT
    c."Id",
    gc."Id",
    t."Id",
    'Spring Invitational – Round 1',
    2,   -- Tournament
    'Net Stroke Play',
    NOW()-INTERVAL '30 days',
    3,   -- Completed
    32, TRUE, TRUE,
    NOW()-INTERVAL '31 days'
FROM "Clubs" c
JOIN "GolfCourses" gc ON gc."ClubId" = c."Id"
JOIN "Tournaments" t  ON t."Name" = 'Spring Invitational 2026'
WHERE c."Slug" = 'pine-valley'
  AND NOT EXISTS (
      SELECT 1 FROM "GolfEvents" WHERE "Name" = 'Spring Invitational – Round 1'
  )
LIMIT 1;

-- =============================================================================
-- 7. Event participants
-- =============================================================================
INSERT INTO "EventParticipants" ("EventId","UserId","RegisteredAt","CheckedIn","CreatedAt")
SELECT
    ev."Id",
    u,
    NOW()-INTERVAL '30 days',
    TRUE,
    NOW()
FROM "GolfEvents" ev
CROSS JOIN unnest(ARRAY['u-001','u-002','u-003','u-004','u-005','u-006','u-007','u-008']::TEXT[]) AS u
WHERE ev."Name" = 'Spring Invitational – Round 1'
  AND NOT EXISTS (
      SELECT 1 FROM "EventParticipants" ep
      WHERE ep."EventId" = ev."Id" AND ep."UserId" = u
  );

-- =============================================================================
-- 8. Rounds (completed)
-- =============================================================================
INSERT INTO "Rounds" (
    "EventId","CourseId","UserId","RoundNumber","Status",
    "GrossScore","NetScore","HandicapUsed",
    "TotalPutts","FairwaysHit","TotalFairways","GreensInRegulation","TotalGreens",
    "Birdies","Pars","Bogeys","DoubleBogeys",
    "IsAttested","StartedAt","CompletedAt","CreatedAt"
)
SELECT
    ev."Id",
    gc."Id",
    r."UserId",
    1,
    2,   -- RoundStatus.Completed
    r."Gross", r."Net", r."Hcp",
    r."Putts", r."FW", 14, r."GIR", 18,
    r."Birdies", r."Pars", r."Bogeys", r."Doubles",
    TRUE,
    NOW()-INTERVAL '30 days',
    NOW()-INTERVAL '29 days 22 hours',
    NOW()
FROM "GolfEvents" ev
JOIN "GolfCourses" gc ON gc."ClubId" = ev."ClubId"
CROSS JOIN (VALUES
    ('u-001',76,72,4.2::DOUBLE PRECISION,30,9,10,2,9,6,1),
    ('u-007',74,72,2.5::DOUBLE PRECISION,29,10,11,3,10,4,1),
    ('u-002',74,72,2.1::DOUBLE PRECISION,28,11,12,4, 9,4,1),
    ('u-006',75,72,3.0::DOUBLE PRECISION,31, 9,10,2,10,5,1),
    ('u-003',79,72,6.8::DOUBLE PRECISION,33, 8, 9,1, 8,8,1),
    ('u-004',76,73,3.5::DOUBLE PRECISION,30, 9,10,2, 9,6,1),
    ('u-005',73,71,1.8::DOUBLE PRECISION,27,12,13,5,10,2,1),
    ('u-008',80,72,8.4::DOUBLE PRECISION,34, 7, 8,1, 7,8,2)
) AS r("UserId","Gross","Net","Hcp","Putts","FW","GIR","Birdies","Pars","Bogeys","Doubles")
WHERE ev."Name" = 'Spring Invitational – Round 1'
  AND NOT EXISTS (
      SELECT 1 FROM "Rounds" rnd
      WHERE rnd."EventId" = ev."Id" AND rnd."UserId" = r."UserId"
  )
LIMIT 8;

-- =============================================================================
-- 9. League Season: Summer Match Play League (active)
-- =============================================================================
INSERT INTO "LeagueSeasons" (
    "ClubId","Name","Format","Status","StartDate","EndDate",
    "MaxParticipants","PointsForWin","PointsForDraw","PointsForLoss",
    "MatchDeadlineDays","UseHandicaps","CreatedAt"
)
SELECT
    c."Id",
    'Summer Match Play League 2026',
    1,   -- MatchPlay
    1,   -- Active
    NOW()-INTERVAL '14 days',
    NOW()+INTERVAL '60 days',
    16, 3, 1, 0, 14, TRUE,
    NOW()-INTERVAL '20 days'
FROM "Clubs" c
WHERE c."Slug" = 'pine-valley'
  AND NOT EXISTS (
      SELECT 1 FROM "LeagueSeasons" WHERE "Name" = 'Summer Match Play League 2026'
  );

-- =============================================================================
-- 10. League entries
-- =============================================================================
INSERT INTO "LeagueEntries" ("LeagueSeasonId","UserId","RegisteredAt","IsActive","CreatedAt")
SELECT
    ls."Id",
    u,
    NOW()-INTERVAL '14 days',
    TRUE,
    NOW()
FROM "LeagueSeasons" ls
CROSS JOIN unnest(ARRAY['u-001','u-002','u-003','u-004','u-005','u-006','u-007','u-008']::TEXT[]) AS u
WHERE ls."Name" = 'Summer Match Play League 2026'
  AND NOT EXISTS (
      SELECT 1 FROM "LeagueEntries" le
      WHERE le."LeagueSeasonId" = ls."Id" AND le."UserId" = u
  );

-- =============================================================================
-- 11. League standings (after round 1)
-- =============================================================================
INSERT INTO "LeagueStandings" (
    "LeagueSeasonId","UserId","Rank","Points","Wins","Losses","Draws","Played","CreatedAt"
)
SELECT
    ls."Id",
    s."UserId",
    s."Rank",
    s."Points",
    s."Wins",
    s."Losses",
    s."Draws",
    1,
    NOW()
FROM "LeagueSeasons" ls
CROSS JOIN (VALUES
    ('u-005',1,3,1,0,0),
    ('u-002',2,3,1,0,0),
    ('u-001',3,1,0,0,1),
    ('u-007',4,1,0,0,1),
    ('u-006',5,0,0,1,0),
    ('u-003',6,0,0,1,0),
    ('u-004',7,0,0,1,0),
    ('u-008',8,0,0,1,0)
) AS s("UserId","Rank","Points","Wins","Losses","Draws")
WHERE ls."Name" = 'Summer Match Play League 2026'
  AND NOT EXISTS (
      SELECT 1 FROM "LeagueStandings" lst
      WHERE lst."LeagueSeasonId" = ls."Id" AND lst."UserId" = s."UserId"
  );

-- =============================================================================
-- 12. Upcoming event: Club Championship Qualifier
-- =============================================================================
INSERT INTO "GolfEvents" (
    "ClubId","CourseId","Name","EventType","Format",
    "EventDate","Status","MaxParticipants","UseHandicaps","IsPublic","CreatedAt"
)
SELECT
    c."Id",
    gc."Id",
    'Club Championship Qualifier',
    0,   -- Regular
    'Gross Stroke Play',
    NOW()+INTERVAL '14 days',
    1,   -- Open
    24, FALSE, TRUE,
    NOW()
FROM "Clubs" c
JOIN "GolfCourses" gc ON gc."ClubId" = c."Id"
WHERE c."Slug" = 'pine-valley'
  AND NOT EXISTS (
      SELECT 1 FROM "GolfEvents" WHERE "Name" = 'Club Championship Qualifier'
  )
LIMIT 1;

-- =============================================================================
-- 13. Club announcements
-- =============================================================================
INSERT INTO "ClubAnnouncements" (
    "ClubId","Title","Body","CreatedByUserId","Target",
    "IsPinned","IsPublished","PublishedAt","CreatedAt"
)
SELECT
    c."Id",
    ann."Title",
    ann."Body",
    (SELECT "Id" FROM "AspNetUsers" WHERE "Email" = 'admin@golfdaytracker.com' LIMIT 1),
    0,   -- AllMembers
    ann."Pinned",
    TRUE,
    NOW(),
    NOW()
FROM "Clubs" c
CROSS JOIN (VALUES
    ('Welcome to the New GolfDay Tracker!',
     'We have launched our new online golf tracking system. You can now register for events, track your scores, and follow the league standings all in one place. Contact the pro shop if you have any questions.',
     TRUE),
    ('Club Championship Qualifier – Register Now',
     'Registration is open for the Club Championship Qualifier in two weeks. Spots are limited to 24 players. Sign up through the Events page.',
     FALSE)
) AS ann("Title","Body","Pinned")
WHERE c."Slug" = 'pine-valley'
  AND NOT EXISTS (
      SELECT 1 FROM "ClubAnnouncements" ca
      WHERE ca."ClubId" = c."Id" AND ca."Title" = ann."Title"
  );

COMMIT;

-- =============================================================================
-- Verification
-- =============================================================================
SELECT 'Users'             AS entity, COUNT(*) AS count FROM "AspNetUsers"
UNION ALL SELECT 'ClubMemberships',   COUNT(*) FROM "ClubMemberships"
UNION ALL SELECT 'Tournaments',       COUNT(*) FROM "Tournaments"
UNION ALL SELECT 'TournamentEntries', COUNT(*) FROM "TournamentEntries"
UNION ALL SELECT 'GolfEvents',        COUNT(*) FROM "GolfEvents"
UNION ALL SELECT 'Rounds',            COUNT(*) FROM "Rounds"
UNION ALL SELECT 'LeagueSeasons',     COUNT(*) FROM "LeagueSeasons"
UNION ALL SELECT 'LeagueStandings',   COUNT(*) FROM "LeagueStandings"
UNION ALL SELECT 'ClubAnnouncements', COUNT(*) FROM "ClubAnnouncements"
ORDER BY entity;
