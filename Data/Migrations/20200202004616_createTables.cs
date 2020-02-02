﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class createTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChrominoInGame",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChrominoId = table.Column<byte>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    Orientation = table.Column<int>(nullable: false),
                    XPosition = table.Column<int>(nullable: false),
                    YPosition = table.Column<int>(nullable: false),
                    ChrominoId1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChrominoInGame", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChrominoInGame_Chromino_ChrominoId1",
                        column: x => x.ChrominoId1,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChrominoInGame_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChrominoInHand",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChrominoId = table.Column<byte>(nullable: false),
                    GameId = table.Column<int>(nullable: false),
                    PlayerId = table.Column<int>(nullable: false),
                    Position = table.Column<byte>(nullable: false),
                    ChrominoId1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChrominoInHand", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChrominoInHand_Chromino_ChrominoId1",
                        column: x => x.ChrominoId1,
                        principalTable: "Chromino",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChrominoInHand_Game_GameId",
                        column: x => x.GameId,
                        principalTable: "Game",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChrominoInHand_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChrominoInGame_ChrominoId1",
                table: "ChrominoInGame",
                column: "ChrominoId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChrominoInGame_GameId",
                table: "ChrominoInGame",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ChrominoInHand_ChrominoId1",
                table: "ChrominoInHand",
                column: "ChrominoId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChrominoInHand_GameId",
                table: "ChrominoInHand",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_ChrominoInHand_PlayerId",
                table: "ChrominoInHand",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChrominoInGame");

            migrationBuilder.DropTable(
                name: "ChrominoInHand");
        }
    }
}
