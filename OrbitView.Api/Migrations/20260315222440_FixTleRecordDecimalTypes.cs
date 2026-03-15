using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrbitView.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixTleRecordDecimalTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MeanMotion",
                table: "TleRecords",
                type: "decimal(12,8)",
                precision: 12,
                scale: 8,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 12,
                oldScale: 8);

            migrationBuilder.AlterColumn<decimal>(
                name: "Inclination",
                table: "TleRecords",
                type: "decimal(8,4)",
                precision: 8,
                scale: 4,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 8,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Eccentricity",
                table: "TleRecords",
                type: "decimal(10,7)",
                precision: 10,
                scale: 7,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 10,
                oldScale: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MeanMotion",
                table: "TleRecords",
                type: "int",
                precision: 12,
                scale: 8,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,8)",
                oldPrecision: 12,
                oldScale: 8);

            migrationBuilder.AlterColumn<int>(
                name: "Inclination",
                table: "TleRecords",
                type: "int",
                precision: 8,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,4)",
                oldPrecision: 8,
                oldScale: 4);

            migrationBuilder.AlterColumn<int>(
                name: "Eccentricity",
                table: "TleRecords",
                type: "int",
                precision: 10,
                scale: 7,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,7)",
                oldPrecision: 10,
                oldScale: 7);
        }
    }
}
