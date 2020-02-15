using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class OpenSides : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OpenBottom",
                table: "Square",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OpenLeft",
                table: "Square",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OpenRight",
                table: "Square",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OpenTop",
                table: "Square",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenBottom",
                table: "Square");

            migrationBuilder.DropColumn(
                name: "OpenLeft",
                table: "Square");

            migrationBuilder.DropColumn(
                name: "OpenRight",
                table: "Square");

            migrationBuilder.DropColumn(
                name: "OpenTop",
                table: "Square");
        }
    }
}
