using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class İlkOlusturma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Berberler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Berberler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Yetenekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yetenekler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BerberYetenek",
                columns: table => new
                {
                    BerberlerId = table.Column<int>(type: "integer", nullable: false),
                    YeteneklerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BerberYetenek", x => new { x.BerberlerId, x.YeteneklerId });
                    table.ForeignKey(
                        name: "FK_BerberYetenek_Berberler_BerberlerId",
                        column: x => x.BerberlerId,
                        principalTable: "Berberler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BerberYetenek_Yetenekler_YeteneklerId",
                        column: x => x.YeteneklerId,
                        principalTable: "Yetenekler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BerberYetenek_YeteneklerId",
                table: "BerberYetenek",
                column: "YeteneklerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BerberYetenek");

            migrationBuilder.DropTable(
                name: "Berberler");

            migrationBuilder.DropTable(
                name: "Yetenekler");
        }
    }
}
