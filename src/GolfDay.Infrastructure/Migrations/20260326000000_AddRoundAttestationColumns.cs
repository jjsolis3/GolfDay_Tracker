using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GolfDay.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoundAttestationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAttested",
                table: "Rounds",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AttestedByUserId",
                table: "Rounds",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttestedAt",
                table: "Rounds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttestationNotes",
                table: "Rounds",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsAttested", table: "Rounds");
            migrationBuilder.DropColumn(name: "AttestedByUserId", table: "Rounds");
            migrationBuilder.DropColumn(name: "AttestedAt", table: "Rounds");
            migrationBuilder.DropColumn(name: "AttestationNotes", table: "Rounds");
        }
    }
}
