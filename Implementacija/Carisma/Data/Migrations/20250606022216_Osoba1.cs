using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carisma.Data.Migrations
{
    /// <inheritdoc />
    public partial class Osoba1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdKorisnika",
                table: "Osoba",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdKorisnika",
                table: "Osoba");
        }
    }
}
