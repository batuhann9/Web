using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BerberSalonu.Migrations
{
    /// <inheritdoc />
    public partial class berberkullanici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Kullanicilar_Roller_RolId",
                table: "Kullanicilar");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "Berberler");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Berberler");

            migrationBuilder.AlterColumn<int>(
                name: "RolId",
                table: "Kullanicilar",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KullaniciId",
                table: "Berberler",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Berberler_KullaniciId",
                table: "Berberler",
                column: "KullaniciId");

            migrationBuilder.AddForeignKey(
                name: "FK_Berberler_Kullanicilar_KullaniciId",
                table: "Berberler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Kullanicilar_Roller_RolId",
                table: "Kullanicilar",
                column: "RolId",
                principalTable: "Roller",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Berberler_Kullanicilar_KullaniciId",
                table: "Berberler");

            migrationBuilder.DropForeignKey(
                name: "FK_Kullanicilar_Roller_RolId",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_Berberler_KullaniciId",
                table: "Berberler");

            migrationBuilder.DropColumn(
                name: "KullaniciId",
                table: "Berberler");

            migrationBuilder.AlterColumn<int>(
                name: "RolId",
                table: "Kullanicilar",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Berberler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Berberler",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Kullanicilar_Roller_RolId",
                table: "Kullanicilar",
                column: "RolId",
                principalTable: "Roller",
                principalColumn: "Id");
        }
    }
}
