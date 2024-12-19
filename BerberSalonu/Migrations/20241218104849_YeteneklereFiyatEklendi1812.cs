using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class YeteneklereFiyatEklendi1812 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Yetenekler",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Yetenekler");
        }
    }
}
