using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ViewFinished : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Win",
                table: "GamePlayer",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<bool>(
                name: "ViewFinished",
                table: "GamePlayer",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ViewFinished",
                table: "GamePlayer");

            migrationBuilder.AlterColumn<bool>(
                name: "Win",
                table: "GamePlayer",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldNullable: true);
        }
    }
}
