using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class NoTips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GamesFinished",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PlayedGames",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<bool>(
                name: "NoTips",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoTips",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "GamesFinished",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayedGames",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
