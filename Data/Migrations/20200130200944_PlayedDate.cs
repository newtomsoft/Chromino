using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Data.Migrations
{
    public partial class PlayedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Guid",
                table: "Game",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlayedDate",
                table: "Game",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayedDate",
                table: "Game");

            migrationBuilder.AlterColumn<string>(
                name: "Guid",
                table: "Game",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 32,
                oldNullable: true);
        }
    }
}
