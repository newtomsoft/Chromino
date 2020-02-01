using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Win : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("PlayerPoints", "GamePlayer", "Points");
            migrationBuilder.RenameColumn("PlayerTurn", "GamePlayer", "Turn");

            migrationBuilder.AddColumn<bool>(
                name: "Win",
                table: "GamePlayer",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn("Points", "GamePlayer", "PlayerPoints");
            migrationBuilder.RenameColumn("Turn", "GamePlayer", "PlayerTurn");

            migrationBuilder.DropColumn(
                name: "Win",
                table: "GamePlayer");
        }
    }
}
