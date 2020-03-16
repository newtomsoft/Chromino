using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class memoAndChat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chat",
                table: "GamePlayer");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "GamePlayer");

            migrationBuilder.AddColumn<string>(
                name: "Memo",
                table: "GamePlayer",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Memo",
                table: "GamePlayer");

            migrationBuilder.AddColumn<string>(
                name: "Chat",
                table: "GamePlayer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "GamePlayer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
