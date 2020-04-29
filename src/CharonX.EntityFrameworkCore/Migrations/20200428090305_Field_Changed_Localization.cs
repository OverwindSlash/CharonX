using Microsoft.EntityFrameworkCore.Migrations;

namespace CharonX.Migrations
{
    public partial class Field_Changed_Localization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Localization",
                table: "CustomPermissionSettings");

            migrationBuilder.DropColumn(
                name: "Localization",
                table: "CustomFeatureSettings");

            migrationBuilder.AddColumn<string>(
                name: "LocalizationEn",
                table: "CustomPermissionSettings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalizationZh",
                table: "CustomPermissionSettings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalizationEn",
                table: "CustomFeatureSettings",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalizationZh",
                table: "CustomFeatureSettings",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocalizationEn",
                table: "CustomPermissionSettings");

            migrationBuilder.DropColumn(
                name: "LocalizationZh",
                table: "CustomPermissionSettings");

            migrationBuilder.DropColumn(
                name: "LocalizationEn",
                table: "CustomFeatureSettings");

            migrationBuilder.DropColumn(
                name: "LocalizationZh",
                table: "CustomFeatureSettings");

            migrationBuilder.AddColumn<string>(
                name: "Localization",
                table: "CustomPermissionSettings",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localization",
                table: "CustomFeatureSettings",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);
        }
    }
}
