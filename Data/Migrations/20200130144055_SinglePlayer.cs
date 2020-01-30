using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class SinglePlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinishedSinglePlayerGames",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointsSinglePlayerGames",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedSinglePlayerGames",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "PointsSinglePlayerGames",
                table: "Player");
        }
    }
}
