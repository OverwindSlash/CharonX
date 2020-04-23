using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CharonX.Migrations
{
    public partial class Add_ExtraMember_To_User_And_Tenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpOrganizationUnits_TenantId_Code",
                table: "AbpOrganizationUnits");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AbpUsers",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "AbpUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "AbpUsers",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdNumber",
                table: "AbpUsers",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficePhoneNumber",
                table: "AbpUsers",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AbpTenants",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminPhoneNumber",
                table: "AbpTenants",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Contact",
                table: "AbpTenants",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "AbpTenants",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnits_TenantId_Code",
                table: "AbpOrganizationUnits",
                columns: new[] { "TenantId", "Code" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpOrganizationUnits_TenantId_Code",
                table: "AbpOrganizationUnits");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "IdNumber",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "OfficePhoneNumber",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "AdminPhoneNumber",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "Contact",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "AbpTenants");

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnits_TenantId_Code",
                table: "AbpOrganizationUnits",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }
    }
}
