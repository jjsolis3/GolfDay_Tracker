using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfDay.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MakeCourseIndependent : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Drop the old required FK
        migrationBuilder.DropForeignKey(
            name: "FK_GolfCourses_Clubs_ClubId",
            table: "GolfCourses");

        // Remove IsHomeClubCourse (replaced by IsPublic)
        migrationBuilder.DropColumn(
            name: "IsHomeClubCourse",
            table: "GolfCourses");

        // Make ClubId nullable
        migrationBuilder.AlterColumn<int>(
            name: "ClubId",
            table: "GolfCourses",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        // Add IsPublic (default true so existing courses are visible)
        migrationBuilder.AddColumn<bool>(
            name: "IsPublic",
            table: "GolfCourses",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        // Re-add FK with SetNull on club delete
        migrationBuilder.AddForeignKey(
            name: "FK_GolfCourses_Clubs_ClubId",
            table: "GolfCourses",
            column: "ClubId",
            principalTable: "Clubs",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_GolfCourses_Clubs_ClubId",
            table: "GolfCourses");

        migrationBuilder.DropColumn(
            name: "IsPublic",
            table: "GolfCourses");

        migrationBuilder.AlterColumn<int>(
            name: "ClubId",
            table: "GolfCourses",
            type: "integer",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsHomeClubCourse",
            table: "GolfCourses",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddForeignKey(
            name: "FK_GolfCourses_Clubs_ClubId",
            table: "GolfCourses",
            column: "ClubId",
            principalTable: "Clubs",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
