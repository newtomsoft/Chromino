using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PlayError : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tip_Tip_NextTipId",
                table: "Tip");

            migrationBuilder.DropIndex(
                name: "IX_Tip_NextTipId",
                table: "Tip");

            migrationBuilder.DropColumn(
                name: "NextTipId",
                table: "Tip");

            migrationBuilder.CreateTable(
                name: "PlayError",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 25, nullable: false),
                    Description = table.Column<string>(nullable: false),
                    IllustrationPictureClass = table.Column<string>(maxLength: 25, nullable: true),
                    IllustrationPictureCaption = table.Column<string>(maxLength: 70, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayError", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayError");

            migrationBuilder.AddColumn<int>(
                name: "NextTipId",
                table: "Tip",
                type: "int",
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
    }
}
