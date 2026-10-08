using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlideMaker.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPresentationBranding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassName",
                table: "Presentations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EventDate",
                table: "Presentations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FestivalName",
                table: "Presentations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderMessage",
                table: "Presentations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorCredit",
                table: "Presentations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassName",
                table: "Presentations");

            migrationBuilder.DropColumn(
                name: "EventDate",
                table: "Presentations");

            migrationBuilder.DropColumn(
                name: "FestivalName",
                table: "Presentations");

            migrationBuilder.DropColumn(
                name: "HeaderMessage",
                table: "Presentations");

            migrationBuilder.DropColumn(
                name: "InstructorCredit",
                table: "Presentations");
        }
    }
}
