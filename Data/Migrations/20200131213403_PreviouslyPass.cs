using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PreviouslyPass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PreviouslyPass",
                table: "GamePlayer",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviouslyPass",
                table: "GamePlayer");
        }
    }
}
