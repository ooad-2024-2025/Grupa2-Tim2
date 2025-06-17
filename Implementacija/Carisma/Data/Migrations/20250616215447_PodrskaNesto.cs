using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class PodrskaNesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatumOdgovoraKorisnika",
                table: "Podrska",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OdgovorKorisnika",
                table: "Podrska",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatumOdgovoraKorisnika",
                table: "Podrska");

            migrationBuilder.DropColumn(
                name: "OdgovorKorisnika",
                table: "Podrska");
        }
    }
}
