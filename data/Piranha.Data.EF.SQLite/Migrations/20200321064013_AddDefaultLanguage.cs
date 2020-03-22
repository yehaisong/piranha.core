using Microsoft.EntityFrameworkCore.Migrations;

namespace Piranha.Data.EF.SQLite.Migrations
{
    public partial class AddDefaultLanguage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Piranha_Languages",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Piranha_Languages");
        }
    }
}
