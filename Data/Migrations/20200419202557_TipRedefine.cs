using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class TipRedefine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PicturePath",
                table: "Tip");

            migrationBuilder.AddColumn<string>(
                name: "HeadPictureClass",
                table: "Tip",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IllustrationPicture",
                table: "Tip",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextTipId",
                table: "Tip",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tip_NextTipId",
                table: "Tip",
                column: "NextTipId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tip_Tip_NextTipId",
                table: "Tip",
                column: "NextTipId",
                principalTable: "Tip",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tip_Tip_NextTipId",
                table: "Tip");

            migrationBuilder.DropIndex(
                name: "IX_Tip_NextTipId",
                table: "Tip");

            migrationBuilder.DropColumn(
                name: "HeadPictureClass",
                table: "Tip");

            migrationBuilder.DropColumn(
                name: "IllustrationPicture",
                table: "Tip");

            migrationBuilder.DropColumn(
                name: "NextTipId",
                table: "Tip");

            migrationBuilder.AddColumn<string>(
                name: "PicturePath",
                table: "Tip",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
