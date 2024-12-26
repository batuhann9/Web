using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class RestApiIcinGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Aciklama",
                table: "Yetenekler",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Aciklama",
                table: "Yetenekler");
        }
    }
}
