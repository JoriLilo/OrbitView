using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbitView.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixCategoryForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Satellites_NoradId",
                table: "Satellites");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Satellites");

            migrationBuilder.RenameColumn(
                name: "ColorHex",
                table: "SatelliteCategories",
                newName: "ColourHex");

            migrationBuilder.AlterColumn<string>(
                name: "CountryOfOrigin",
                table: "Satellites",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ColourHex",
                table: "SatelliteCategories",
                newName: "ColorHex");

            migrationBuilder.UpdateData(
                table: "Satellites",
                keyColumn: "CountryOfOrigin",
                keyValue: null,
                column: "CountryOfOrigin",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CountryOfOrigin",
                table: "Satellites",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Satellites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Satellites_NoradId",
                table: "Satellites",
                column: "NoradId",
                unique: true);
        }
    }
}
