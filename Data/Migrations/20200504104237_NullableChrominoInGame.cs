using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class NullableChrominoInGame : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChrominoInGame_Chromino_ChrominoId",
                table: "ChrominoInGame");

            migrationBuilder.AlterColumn<int>(
                name: "ChrominoId",
                table: "ChrominoInGame",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ChrominoInGame_Chromino_ChrominoId",
                table: "ChrominoInGame",
                column: "ChrominoId",
                principalTable: "Chromino",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChrominoInGame_Chromino_ChrominoId",
                table: "ChrominoInGame");

            migrationBuilder.AlterColumn<int>(
                name: "ChrominoId",
                table: "ChrominoInGame",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChrominoInGame_Chromino_ChrominoId",
                table: "ChrominoInGame",
                column: "ChrominoId",
                principalTable: "Chromino",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
