using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class GamePlayer_Chat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Chat",
                table: "GamePlayer",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Chat",
                table: "Game",
                maxLength: 5000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chat",
                table: "GamePlayer");

            migrationBuilder.DropColumn(
                name: "Chat",
                table: "Game");
        }
    }
}
