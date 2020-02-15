using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class supprEdge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Edge",
                table: "Square");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Edge",
                table: "Square",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
