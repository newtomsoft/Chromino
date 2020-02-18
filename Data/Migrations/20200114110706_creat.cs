using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Data.Migrations
{
    public partial class creat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chromino",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstColor = table.Column<int>(nullable: false),
                    SecondColor = table.Column<int>(nullable: false),
                    ThirdColor = table.Column<int>(nullable: false),
                    Points = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chromino", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Game",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Guid = table.Column<string>(maxLength: 32, nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    AutoPlay = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Game", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pseudo = table.Column<string>(nullable: false),
                    Password = table.Column<string>(maxLength: 64, nullable: false),
                    PlayedGames = table.Column<int>(nullable: false),
                    WonGames = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Square",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(nullable: false),
                    X = table.Column<int>(nullable: false),
                    Y = table.Column<int>(nullable: false),
                    Color = table.Column<int>(nullable: false),
                    Edge = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Square", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Square_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameChromino",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChrominoId = table.Column<int>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Orientation = table.Column<int>(nullable: true),
                    XPosition = table.Column<int>(nullable: true),
                    YPosition = table.Column<int>(nullable: true),
                    PlayerId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameChromino", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameChromino_Chromino_ChrominoId",
                        column: x => x.ChrominoId,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameChromino_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameChromino_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GamePlayer",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    PlayerTurn = table.Column<bool>(nullable: false),
                    PlayerPoints = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePlayer_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GamePlayer_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameChromino_ChrominoId",
                table: "GameChromino",
                column: "ChrominoId");

            migrationBuilder.CreateIndex(
                name: "IX_GameChromino_GameId",
                table: "GameChromino",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GameChromino_PlayerId",
                table: "GameChromino",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayer_GameId",
                table: "GamePlayer",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayer_PlayerId",
                table: "GamePlayer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "NIX_PlayerPseudo",
                table: "Player",
                column: "Pseudo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Square_GameId",
                table: "Square",
                column: "GameId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameChromino");

            migrationBuilder.DropTable(
                name: "GamePlayer");

            migrationBuilder.DropTable(
                name: "Square");

            migrationBuilder.DropTable(
                name: "Chromino");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Game");
        }
    }
}
