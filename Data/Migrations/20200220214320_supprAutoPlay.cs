using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class supprAutoPlay : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoPlay",
                table: "Game");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoPlay",
                table: "Game",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
