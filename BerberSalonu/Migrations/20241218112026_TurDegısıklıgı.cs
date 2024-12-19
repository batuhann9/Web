using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class TurDegısıklıgı : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Sure",
                table: "Yetenekler",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Sure",
                table: "Yetenekler",
                type: "integer",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");
        }
    }
}
