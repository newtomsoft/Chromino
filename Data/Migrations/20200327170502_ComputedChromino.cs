using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ComputedChromino : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComputedChromino",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChrominoId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    BotId = table.Column<int>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    XPosition = table.Column<int>(nullable: false),
                    YPosition = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputedChromino", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComputedChromino_AspNetUsers_BotId",
                        column: x => x.BotId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputedChromino_Chromino_ChrominoId",
                        column: x => x.ChrominoId,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputedChromino_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComputedChromino_ComputedChromino_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ComputedChromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChromino_BotId",
                table: "ComputedChromino",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChromino_ChrominoId",
                table: "ComputedChromino",
                column: "ChrominoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChromino_GameId",
                table: "ComputedChromino",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChromino_ParentId",
                table: "ComputedChromino",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComputedChromino");
        }
    }
}
