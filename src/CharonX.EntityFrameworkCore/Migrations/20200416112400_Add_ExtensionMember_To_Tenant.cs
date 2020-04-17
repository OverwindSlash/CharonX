using Microsoft.EntityFrameworkCore.Migrations;

namespace CharonX.Migrations
{
    public partial class Add_ExtensionMember_To_Tenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
