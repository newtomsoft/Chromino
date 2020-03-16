using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class GamePlayer_note : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "GamePlayer",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "GamePlayer");
        }
    }
}
