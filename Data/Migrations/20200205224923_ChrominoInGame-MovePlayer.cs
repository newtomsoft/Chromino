using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ChrominoInGameMovePlayer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameChromino");

            migrationBuilder.AddColumn<byte>(
                name: "Move",
                table: "ChrominoInGame",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "ChrominoInGame",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChrominoInGame_PlayerId",
                table: "ChrominoInGame",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChrominoInGame_Player_PlayerId",
                table: "ChrominoInGame",
                column: "PlayerId",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChrominoInGame_Player_PlayerId",
                table: "ChrominoInGame");

            migrationBuilder.DropIndex(
                name: "IX_ChrominoInGame_PlayerId",
                table: "ChrominoInGame");

            migrationBuilder.DropColumn(
                name: "Move",
                table: "ChrominoInGame");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "ChrominoInGame");

            migrationBuilder.CreateTable(
                name: "GameChromino",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChrominoId = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false),
                    Orientation = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    XPosition = table.Column<int>(type: "int", nullable: true),
                    YPosition = table.Column<int>(type: "int", nullable: true)
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
        }
    }
}
