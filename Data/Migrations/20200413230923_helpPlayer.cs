using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class helpPlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Help",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Help",
                table: "AspNetUsers");
        }
    }
}
