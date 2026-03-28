using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfDay.Infrastructure.Migrations;

/// <summary>
/// The MakeCourseIndependent migration (20260326500000) was recorded in
/// __EFMigrationsHistory before the IsPublic column was actually applied to
/// the GolfCourses table.  This migration corrects the database state.
/// All statements are idempotent (IF EXISTS / IF NOT EXISTS).
/// </summary>
public partial class FixGolfCourseSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Drop IsHomeClubCourse if it still exists
        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                DROP COLUMN IF EXISTS ""IsHomeClubCourse"";
        ");

        // 2. Make ClubId nullable if it is still NOT NULL
        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM information_schema.columns
                    WHERE table_name  = 'GolfCourses'
                      AND column_name = 'ClubId'
                      AND is_nullable = 'NO'
                ) THEN
                    ALTER TABLE ""GolfCourses""
                        ALTER COLUMN ""ClubId"" DROP NOT NULL;
                END IF;
            END $$;
        ");

        // 3. Add IsPublic (IF NOT EXISTS – idempotent)
        migrationBuilder.Sql(@"
            ALTER TABLE ""GolfCourses""
                ADD COLUMN IF NOT EXISTS ""IsPublic"" boolean NOT NULL DEFAULT true;
        ");

        // 4. Fix FK: drop CASCADE version (if present) and re-add as SET NULL
        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM   information_schema.referential_constraints rc
                    JOIN   information_schema.table_constraints tc
                           ON rc.constraint_name = tc.constraint_name
                    WHERE  tc.constraint_name = 'FK_GolfCourses_Clubs_ClubId'
                      AND  rc.delete_rule      = 'CASCADE'
                ) THEN
                    ALTER TABLE ""GolfCourses""
                        DROP CONSTRAINT ""FK_GolfCourses_Clubs_ClubId"";
                END IF;
            END $$;
        ");

        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM information_schema.table_constraints
                    WHERE  constraint_name = 'FK_GolfCourses_Clubs_ClubId'
                      AND  table_name      = 'GolfCourses'
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

    protected override void Down(MigrationBuilder migrationBuilder)
    {
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
    }
}
