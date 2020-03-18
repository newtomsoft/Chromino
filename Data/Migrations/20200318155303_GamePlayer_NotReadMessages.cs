using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class GamePlayer_NotReadMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "NotReadMessages",
                table: "GamePlayer",
                nullable: false,
                defaultValue: (byte)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotReadMessages",
                table: "GamePlayer");
        }
    }
}
