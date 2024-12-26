using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class RandevuIcinIkiFarkliDurumEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnaylandi",
                table: "Randevular");

            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "Randevular",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Randevular");

            migrationBuilder.AddColumn<bool>(
                name: "IsOnaylandi",
                table: "Randevular",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
