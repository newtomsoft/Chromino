using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class PrivateMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DomElementId",
                newName: "Name",
                table: "Tip");

            migrationBuilder.CreateTable(
                name: "PrivatesMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(nullable: false),
                    RecipientId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivatesMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivatesMessages_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrivatesMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PrivatesMessagesLatestRead",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientId = table.Column<int>(nullable: false),
                    SenderId = table.Column<int>(nullable: false),
                    LatestRead = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivatesMessagesLatestRead", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivatesMessagesLatestRead_AspNetUsers_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrivatesMessagesLatestRead_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivatesMessages_RecipientId",
                table: "PrivatesMessages",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivatesMessages_SenderId",
                table: "PrivatesMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivatesMessagesLatestRead_RecipientId",
                table: "PrivatesMessagesLatestRead",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivatesMessagesLatestRead_SenderId",
                table: "PrivatesMessagesLatestRead",
                column: "SenderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivatesMessages");

            migrationBuilder.DropTable(
                name: "PrivatesMessagesLatestRead");

            migrationBuilder.RenameColumn(
                name: "Name",
                newName: "DomElementId", 
                table: "Tip");
        }
    }
}
