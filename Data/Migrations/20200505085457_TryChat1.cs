using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class TryChat1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotReadMessages",
                table: "GamePlayer");

            migrationBuilder.DropColumn(
                name: "Chat",
                table: "Game");

            migrationBuilder.AddColumn<DateTime>(
                name: "LatestReadMessage",
                table: "GamePlayer",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GamePlayerId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chats_GamePlayer_GamePlayerId",
                        column: x => x.GamePlayerId,
                        principalTable: "GamePlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_GamePlayerId",
                table: "Chats",
                column: "GamePlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropColumn(
                name: "LatestReadMessage",
                table: "GamePlayer");

            migrationBuilder.AddColumn<byte>(
                name: "NotReadMessages",
                table: "GamePlayer",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "Chat",
                table: "Game",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true);
        }
    }
}
