using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class Tips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tip",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipName = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    IllustrationImagePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tip", x => x.Id);
                    table.UniqueConstraint("UQ_Tip", x => x.TipName);
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
        }
    }
}
