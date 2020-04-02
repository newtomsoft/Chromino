using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class computedChrominoOk : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComputedChrominoOk",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    ChrominoId = table.Column<int>(nullable: false),
                    Level = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputedChrominoOk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComputedChrominoOk_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputedChrominoOk_Chromino_ChrominoId",
                        column: x => x.ChrominoId,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputedChrominoOk_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChrominoOk_PlayerId",
                table: "ComputedChrominoOk",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChrominoOk_ChrominoId",
                table: "ComputedChrominoOk",
                column: "ChrominoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChrominoOk_GameId",
                table: "ComputedChrominoOk",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComputedChrominoOk");
        }
    }
}
