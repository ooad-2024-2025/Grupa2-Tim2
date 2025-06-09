using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class VoziloRezervacija : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dostupnost",
                table: "Vozilo");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Vozilo",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vozilo");

            migrationBuilder.AddColumn<bool>(
                name: "Dostupnost",
                table: "Vozilo",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
