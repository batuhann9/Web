using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class KorkuncİslerDeniyoruz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BerberYetenekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BerberId = table.Column<int>(type: "integer", nullable: false),
                    YetenekId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BerberYetenekler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BerberYetenekler_Berberler_BerberId",
                        column: x => x.BerberId,
                        principalTable: "Berberler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BerberYetenekler_Yetenekler_YetenekId",
                        column: x => x.YetenekId,
                        principalTable: "Yetenekler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BerberYetenekler_BerberId",
                table: "BerberYetenekler",
                column: "BerberId");

            migrationBuilder.CreateIndex(
                name: "IX_BerberYetenekler_YetenekId",
                table: "BerberYetenekler",
                column: "YetenekId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BerberYetenekler");
        }
    }
}
