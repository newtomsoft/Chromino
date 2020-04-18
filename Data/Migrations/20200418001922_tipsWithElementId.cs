using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class tipsWithElementId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IllustrationImagePath",
                table: "Tip");

            migrationBuilder.AddColumn<string>(
                name: "DomElementId",
                table: "Tip",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PicturePath",
                table: "Tip",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DomElementId",
                table: "Tip");

            migrationBuilder.DropColumn(
                name: "PicturePath",
                table: "Tip");

            migrationBuilder.AddColumn<string>(
                name: "IllustrationImagePath",
                table: "Tip",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
