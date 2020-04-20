using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Tips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoTips",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Tip",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DomElementId = table.Column<string>(nullable: false),
                    HeadPictureClass = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    IllustrationPictureClass = table.Column<string>(nullable: true),
                    NextTipId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tip", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tip_Tip_NextTipId",
                        column: x => x.NextTipId,
                        principalTable: "Tip",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TipOff",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(nullable: false),
                    TipId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipOff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TipOff_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TipOff_Tip_TipId",
                        column: x => x.TipId,
                        principalTable: "Tip",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tip_NextTipId",
                table: "Tip",
                column: "NextTipId");

            migrationBuilder.CreateIndex(
                name: "IX_TipOff_PlayerId",
                table: "TipOff",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TipOff_TipId",
                table: "TipOff",
                column: "TipId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TipOff");

            migrationBuilder.DropTable(
                name: "Tip");

            migrationBuilder.AddColumn<bool>(
                name: "NoTips",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
