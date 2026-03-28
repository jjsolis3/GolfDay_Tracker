using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfDay.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MakeCourseIndependent : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Use raw SQL with IF EXISTS / IF NOT EXISTS so this migration is
        // idempotent and won't fail if the DB is already in a partial state.

        // 1. Drop the required FK (IF EXISTS – Supabase may name it differently)
        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.table_constraints
                    WHERE constraint_name = 'FK_GolfCourses_Clubs_ClubId'
                      AND table_name = 'GolfCourses'
                ) THEN
                    ALTER TABLE ""GolfCourses""
                        DROP CONSTRAINT ""FK_GolfCourses_Clubs_ClubId"";
                END IF;
            END $$;
        ");

        // 2. Drop old IsHomeClubCourse column (IF EXISTS)
        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                DROP COLUMN IF EXISTS ""IsHomeClubCourse"";
        ");

        // 3. Make ClubId nullable (safe to run even if already nullable)
        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                ALTER COLUMN ""ClubId"" DROP NOT NULL;
        ");

        // 4. Add IsPublic (IF NOT EXISTS – won't fail if column already added)
        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                ADD COLUMN IF NOT EXISTS ""IsPublic"" boolean NOT NULL DEFAULT true;
        ");

        // 5. Re-add FK with ON DELETE SET NULL (IF NOT EXISTS guard via DO block)
        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.table_constraints
                    WHERE constraint_name = 'FK_GolfCourses_Clubs_ClubId'
                      AND table_name = 'GolfCourses'
                ) THEN
                    ALTER TABLE ""GolfCourses""
                        ADD CONSTRAINT ""FK_GolfCourses_Clubs_ClubId""
                        FOREIGN KEY (""ClubId"")
                        REFERENCES ""Clubs""(""Id"")
                        ON DELETE SET NULL;
                END IF;
            END $$;
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                DROP CONSTRAINT IF EXISTS ""FK_GolfCourses_Clubs_ClubId"";
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                DROP COLUMN IF EXISTS ""IsPublic"";
        ");

        migrationBuilder.Sql(@"
            UPDATE ""GolfCourses"" SET ""ClubId"" = 0 WHERE ""ClubId"" IS NULL;
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                ALTER COLUMN ""ClubId"" SET NOT NULL;
        ");

        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                ADD COLUMN IF NOT EXISTS ""IsHomeClubCourse"" boolean NOT NULL DEFAULT false;
        ");

        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.table_constraints
                    WHERE constraint_name = 'FK_GolfCourses_Clubs_ClubId'
                      AND table_name = 'GolfCourses'
                ) THEN
                    ALTER TABLE ""GolfCourses""
                        ADD CONSTRAINT ""FK_GolfCourses_Clubs_ClubId""
                        FOREIGN KEY (""ClubId"")
                        REFERENCES ""Clubs""(""Id"")
                        ON DELETE CASCADE;
                END IF;
            END $$;
        ");
    }
}
