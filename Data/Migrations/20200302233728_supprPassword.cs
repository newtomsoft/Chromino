using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class supprPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinishedSinglePlayerGames",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PointsSinglePlayerGames",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Pseudo",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "GamesFinished",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SinglePlayerGamesFinished",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SinglePlayerGamesPoints",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GamesFinished",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SinglePlayerGamesFinished",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SinglePlayerGamesPoints",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "FinishedSinglePlayerGames",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PointsSinglePlayerGames",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Pseudo",
                table: "AspNetUsers",
                type: "varchar(25)",
                nullable: false,
                defaultValue: "");
        }
    }
}
