using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenBeerMenu.Migrations
{
    public partial class AddExternalSyncProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "sync_enabled",
                table: "settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "sync_key",
                table: "settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sync_url",
                table: "settings",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sync_enabled",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "sync_key",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "sync_url",
                table: "settings");
        }
    }
}
