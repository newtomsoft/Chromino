using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class autoplay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoPlay",
                table: "Game",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoPlay",
                table: "Game");
        }
    }
}
