using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GolfDay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseTeeBoxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GolfCourseTeeBoxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GolfCourseId  = table.Column<int>(type: "integer", nullable: false),
                    TeeBoxName    = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Gender        = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Par           = table.Column<int>(type: "integer", nullable: false, defaultValue: 72),
                    CourseRating  = table.Column<double>(type: "double precision", nullable: true),
                    SlopeRating   = table.Column<double>(type: "double precision", nullable: true),
                    TotalYards    = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt     = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt     = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GolfCourseTeeBoxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GolfCourseTeeBoxes_GolfCourses_GolfCourseId",
                        column: x => x.GolfCourseId,
                        principalTable: "GolfCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GolfCourseTeeBoxes_GolfCourseId",
                table: "GolfCourseTeeBoxes",
                column: "GolfCourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "GolfCourseTeeBoxes");
        }
    }
}
