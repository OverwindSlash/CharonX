using Microsoft.EntityFrameworkCore.Migrations;

namespace CharonX.Migrations
{
    public partial class Add_LogoNode_To_Tenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoNode",
                table: "AbpTenants",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoNode",
                table: "AbpTenants");
        }
    }
}
