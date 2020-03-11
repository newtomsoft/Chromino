using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Game_Move : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "Move",
                table: "Game",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Move",
                table: "Game");
        }
    }
}
