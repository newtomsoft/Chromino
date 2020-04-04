using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class rename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComputedChromino");

            migrationBuilder.DropTable(
                name: "ComputedChrominoOk");

            migrationBuilder.CreateTable(
                name: "GoodPosition",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChrominoId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    Flip = table.Column<bool>(nullable: false),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodPosition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodPosition_Chromino_ChrominoId",
                        column: x => x.ChrominoId,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodPosition_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodPosition_GoodPosition_ParentId",
                        column: x => x.ParentId,
                        principalTable: "GoodPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodPosition_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GoodPositionLevel",
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
                    table.PrimaryKey("PK_GoodPositionLevel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodPositionLevel_Chromino_ChrominoId",
                        column: x => x.ChrominoId,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodPositionLevel_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodPositionLevel_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodPosition_ChrominoId",
                table: "GoodPosition",
                column: "ChrominoId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodPosition_GameId",
                table: "GoodPosition",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodPosition_ParentId",
                table: "GoodPosition",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodPosition_PlayerId",
                table: "GoodPosition",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodPositionLevel_ChrominoId",
                table: "GoodPositionLevel",
                column: "ChrominoId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodPositionLevel_GameId",
                table: "GoodPositionLevel",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodPositionLevel_PlayerId",
                table: "GoodPositionLevel",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodPosition");

            migrationBuilder.DropTable(
                name: "GoodPositionLevel");

            migrationBuilder.CreateTable(
                name: "ComputedChromino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BotId = table.Column<int>(type: "int", nullable: false),
                    ChrominoId = table.Column<int>(type: "int", nullable: false),
                    Flip = table.Column<bool>(type: "bit", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Orientation = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ComputedChrominoOk",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BotId = table.Column<int>(type: "int", nullable: false),
                    ChrominoId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComputedChrominoOk", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComputedChrominoOk_AspNetUsers_BotId",
                        column: x => x.BotId,
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

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChrominoOk_BotId",
                table: "ComputedChrominoOk",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChrominoOk_ChrominoId",
                table: "ComputedChrominoOk",
                column: "ChrominoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComputedChrominoOk_GameId",
                table: "ComputedChrominoOk",
                column: "GameId");
        }
    }
}
