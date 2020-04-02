using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class flip : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Flip",
                table: "ComputedChromino",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Flip",
                table: "ChrominoInGame",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flip",
                table: "ComputedChromino");

            migrationBuilder.DropColumn(
                name: "Flip",
                table: "ChrominoInGame");
        }
    }
}
