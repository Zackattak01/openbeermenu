using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OpenBeerMenu.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "beers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    style = table.Column<string>(type: "text", nullable: true),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    abv = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_beers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    show_header = table.Column<bool>(type: "boolean", nullable: false),
                    beer_info_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_sections_beers_beer_info_id",
                        column: x => x.beer_info_id,
                        principalTable: "beers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    logo_url = table.Column<string>(type: "text", nullable: true),
                    access_code = table.Column<string>(type: "text", nullable: true),
                    show_company_header = table.Column<bool>(type: "boolean", nullable: false),
                    default_menu_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_settings_menus_default_menu_id",
                        column: x => x.default_menu_id,
                        principalTable: "menus",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "menu_section",
                columns: table => new
                {
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_menu_section", x => new { x.menu_id, x.section_id });
                    table.ForeignKey(
                        name: "fk_menu_section_menus_menu_id",
                        column: x => x.menu_id,
                        principalTable: "menus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_menu_section_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "section_beer",
                columns: table => new
                {
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    beer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_section_beer", x => new { x.section_id, x.beer_id });
                    table.ForeignKey(
                        name: "fk_section_beer_beers_beer_id",
                        column: x => x.beer_id,
                        principalTable: "beers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_section_beer_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_menu_section_section_id",
                table: "menu_section",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_section_beer_beer_id",
                table: "section_beer",
                column: "beer_id");

            migrationBuilder.CreateIndex(
                name: "ix_sections_beer_info_id",
                table: "sections",
                column: "beer_info_id");

            migrationBuilder.CreateIndex(
                name: "ix_settings_default_menu_id",
                table: "settings",
                column: "default_menu_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "menu_section");

            migrationBuilder.DropTable(
                name: "section_beer");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "sections");

            migrationBuilder.DropTable(
                name: "menus");

            migrationBuilder.DropTable(
                name: "beers");
        }
    }
}
