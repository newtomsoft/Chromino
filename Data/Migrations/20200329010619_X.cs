using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class X : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "XPosition",
                table: "ComputedChromino");

            migrationBuilder.DropColumn(
                name: "YPosition",
                table: "ComputedChromino");

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "ComputedChromino",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "ComputedChromino",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "X",
                table: "ComputedChromino");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "ComputedChromino");

            migrationBuilder.AddColumn<int>(
                name: "XPosition",
                table: "ComputedChromino",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YPosition",
                table: "ComputedChromino",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
