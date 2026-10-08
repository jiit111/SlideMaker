using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlideMaker.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSlideElementStyling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Background",
                table: "SlideElements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Bold",
                table: "SlideElements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "BorderRadius",
                table: "SlideElements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Padding",
                table: "SlideElements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "SlideElements",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Background",
                table: "SlideElements");

            migrationBuilder.DropColumn(
                name: "Bold",
                table: "SlideElements");

            migrationBuilder.DropColumn(
                name: "BorderRadius",
                table: "SlideElements");

            migrationBuilder.DropColumn(
                name: "Padding",
                table: "SlideElements");

            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "SlideElements");
        }
    }
}
