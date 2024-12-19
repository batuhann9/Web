using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class YeteneklereSureEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sure",
                table: "Yetenekler",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sure",
                table: "Yetenekler");
        }
    }
}
